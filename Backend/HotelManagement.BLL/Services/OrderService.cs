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
        var booking = await _bookingRepository.GetByIdAsync(dto.BookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");

        if (booking.PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("Cannot add food orders to a booking that has already been paid.");

        if (booking.BookingStatus != BookingStatus.CheckedIn)
            throw new InvalidOperationException("Food orders can only be placed for guests currently checked in.");

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

        var order = new FoodOrder
        {
            BookingId = dto.BookingId,
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
                PriceAtPurchase = menuItem.Price // Lock in price at time of purchase
            });

            orderDetailsList.Add($"{menuItem.Name} * {itemDto.Quantity}");
        }

        await _foodOrderRepository.AddAsync(order);
        await _foodOrderRepository.SaveChangesAsync();

        var rooms = string.Join(", ", booking.BookingRooms.Where(br => br.RoomId.HasValue).Select(br => br.RoomId!.Value));
        var roomMessage = !string.IsNullOrEmpty(rooms) ? $"Rooms {rooms}" : $"Booking #{booking.Id}";
        var orderDetailsString = string.Join(" , ", orderDetailsList);
        await _notificationService.SendKitchenAlertAsync($"New room service order placed for {roomMessage}.\nOrder : {orderDetailsString}");

        // Return DTO. We must manually map the items because EF might not load the related MenuItems immediately on insert
        var responseDto = _mapper.Map<FoodOrderDTO>(order);
        return responseDto;
    }
}
