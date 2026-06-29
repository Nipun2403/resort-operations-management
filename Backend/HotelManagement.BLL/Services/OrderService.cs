using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;

namespace HotelManagement.BLL.Services;

public class OrderService : IOrderService
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public OrderService(
        IFoodOrderRepository foodOrderRepository,
        IMenuItemRepository menuItemRepository,
        IBookingRepository bookingRepository,
        IMapper mapper,
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _foodOrderRepository = foodOrderRepository;
        _menuItemRepository = menuItemRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<FoodOrderDTO>> GetActiveOrdersAsync(int pageNumber, int pageSize, int? bookingId = null, string? sortBy = null, bool sortDescending = false)
    {
        var ordersList = await _foodOrderRepository.GetActiveOrdersWithDetailsAsync();
        var orders = ordersList.AsQueryable();

        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk") || _currentUserService.IsInRole("Kitchen");
        if (!isStaff)
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                throw new UnauthorizedAccessException("You must be logged in.");

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) throw new ArgumentException("User not found.");

            orders = orders.Where(o => o.Booking != null && o.Booking.UserId == user.Id);
        }

        if (bookingId.HasValue)
            orders = orders.Where(o => o.BookingId == bookingId.Value);

        if (!string.IsNullOrEmpty(sortBy))
            orders = orders.OrderByDynamic(sortBy, sortDescending);
        var totalCount = orders.Count();
        var pagedOrders = orders.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var dtos = _mapper.Map<IEnumerable<FoodOrderDTO>>(pagedOrders);
        return new PaginatedResult<FoodOrderDTO>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = dtos
        };
    }

    public async Task<PaginatedResult<FoodOrderDTO>> GetAllOrdersAsync(int pageNumber, int pageSize, int? bookingId = null, string? status = null, string? sortBy = null, bool sortDescending = false)
    {
        var ordersList = await _foodOrderRepository.GetAllOrdersWithDetailsAsync();
        var orders = ordersList.AsQueryable();

        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk") || _currentUserService.IsInRole("Kitchen");
        if (!isStaff)
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                throw new UnauthorizedAccessException("You must be logged in.");

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) throw new ArgumentException("User not found.");

            orders = orders.Where(o => o.Booking != null && o.Booking.UserId == user.Id);
        }

        if (bookingId.HasValue)
            orders = orders.Where(o => o.BookingId == bookingId.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<FoodOrderStatus>(status, true, out var parsedStatus))
        {
            orders = orders.Where(o => o.OrderStatus == parsedStatus);
        }

        if (!string.IsNullOrEmpty(sortBy))
            orders = orders.OrderByDynamic(sortBy, sortDescending);

        var totalCount = orders.Count();
        var pagedOrders = orders.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var dtos = _mapper.Map<IEnumerable<FoodOrderDTO>>(pagedOrders);
        return new PaginatedResult<FoodOrderDTO>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = dtos
        };
    }

    public async Task UpdateOrderStatusAsync(int orderId, FoodOrderStatus status)
    {
        var order = await _foodOrderRepository.GetByIdAsync(orderId);
        if (order != null)
        {
            order.OrderStatus = status;
            if (status == FoodOrderStatus.Delivered)
            {
                order.FinishedAt = DateTime.UtcNow;
            }
            _foodOrderRepository.Update(order);
            await _foodOrderRepository.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Food Order not found.");
        }
    }

    public async Task<FoodOrderDTO> GetOrderByIdAsync(int orderId)
    {
        var orders = await _foodOrderRepository.GetAllOrdersWithDetailsAsync();
        var order = orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) throw new KeyNotFoundException("Food Order not found.");
        return _mapper.Map<FoodOrderDTO>(order);
    }

    public async Task<FoodOrderDTO> CreateOrderAsync(CreateFoodOrderDTO dto)
    {
        // 1. Fetch the booking (with its rooms)
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(dto.BookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");

        // 2. ✅ VALIDATE: The provided RoomId must belong to this booking
        var roomExists = booking.BookingRooms?.Any(br => br.RoomId == dto.RoomId) ?? false;
        if (!roomExists)
            throw new ArgumentException("The specified room does not belong to this booking.");

        // 3. Validate payment and check-in status
        if (booking.PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("Cannot add food orders to a booking that has already been paid.");

        if (booking.BookingStatus != BookingStatus.CheckedIn)
            throw new InvalidOperationException("Food orders can only be placed for guests currently checked in.");

        // 4. Prevent duplicate identical pending orders
        var activeOrders = await _foodOrderRepository.GetActiveOrdersWithDetailsAsync();
        var pendingOrdersForBooking = activeOrders.Where(o => o.BookingId == dto.BookingId && o.OrderStatus == FoodOrderStatus.Pending).ToList();
        var newOrderItems = dto.Items.OrderBy(i => i.MenuItemId).ToList();

        foreach (var pendingOrder in pendingOrdersForBooking)
        {
            var existingItems = pendingOrder.OrderItems.OrderBy(i => i.MenuItemId).ToList();
            if (existingItems.Count == newOrderItems.Count)
            {
                bool isIdentical = true;
                for (int i = 0; i < existingItems.Count; i++)
                {
                    if (existingItems[i].MenuItemId != newOrderItems[i].MenuItemId || existingItems[i].Quantity != newOrderItems[i].Quantity)
                    {
                        isIdentical = false;
                        break;
                    }
                }
                if (isIdentical)
                {
                    throw new InvalidOperationException("An identical food order is already pending for this booking.");
                }
            }
        }

        // 5. Create the order – ✅ SET THE ROOM ID
        var order = new FoodOrder
        {
            BookingId = dto.BookingId,
            RoomId = dto.RoomId,                     // ✅ Saves the room
            GeneratedAt = DateTime.UtcNow,
            OrderStatus = FoodOrderStatus.Pending
        };

        var orderDetailsList = new List<string>();

        foreach (var itemDto in dto.Items)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(itemDto.MenuItemId);
            if (menuItem == null) throw new ArgumentException($"Menu item {itemDto.MenuItemId} not found.");
            if (!menuItem.IsAvailable) throw new ArgumentException($"Menu item {menuItem.Name} is currently unavailable.");

            order.OrderItems.Add(new FoodOrderItem
            {
                MenuItemId = itemDto.MenuItemId,
                Quantity = itemDto.Quantity,
                PriceAtPurchase = menuItem.Price
            });

            orderDetailsList.Add($"{menuItem.Name} * {itemDto.Quantity}");
        }

        await _foodOrderRepository.AddAsync(order);
        await _foodOrderRepository.SaveChangesAsync();

        // 6. ✅ Reload the order with Room loaded so we can get the RoomNumber
        var createdOrder = await _foodOrderRepository.GetOrderWithDetailsByIdAsync(order.Id);
        var roomNumber = createdOrder?.Room?.RoomNumber ?? "Unknown";

        // 7. ✅ UPDATED KITCHEN ALERT – now shows the exact room number
        var orderDetailsString = string.Join(" , ", orderDetailsList);
        await _notificationService.SendKitchenAlertAsync(
            $"New room service order placed for Room {roomNumber} (Booking #{booking.Id}).\nOrder: {orderDetailsString}"
        );

        // 8. Return DTO
        return _mapper.Map<FoodOrderDTO>(createdOrder);
    }
}