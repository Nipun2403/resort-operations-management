using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Utilities;
namespace HotelManagement.BLL.Services;

public class BillingService : IBillingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IReceiptRepository _receiptRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public BillingService(IBookingRepository bookingRepository, IReceiptRepository receiptRepository, IMapper mapper, ICurrentUserService currentUserService, IUserRepository userRepository, IEmailService emailService)
    {
        _bookingRepository = bookingRepository;
        _receiptRepository = receiptRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _emailService = emailService;
    }
    public async Task<BillingFolioDTO> GenerateFolioAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
        if (booking == null) throw new KeyNotFoundException("Booking not found.");

        var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
        if (nights <= 0) nights = 1;

        var roomTypeNames = booking.BookingRooms
            .Select(br => br.Room?.RoomType?.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList();

        var folio = new BillingFolioDTO
        {
            BookingId = booking.Id,
            GuestName = booking.GuestName,
            NightsStayed = nights,
            RoomBasePrice = booking.BookingRooms.Sum(br => br.LockedInPrice > 0 ? br.LockedInPrice : (br.Room?.RoomType?.BasePrice ?? 0m)),
            FoodTotal = booking.FoodOrders.SelectMany(fo => fo.OrderItems).Sum(fi => fi.PriceAtPurchase * fi.Quantity),
            AmenityTotal = booking.BookingAmenities.Sum(ba => ba.PriceAtPurchase),
            PaymentStatus = booking.BookingStatus == DAL.Enums.BookingStatus.Cancelled ? (booking.PaymentStatus == DAL.Enums.PaymentStatus.Paid ? "Refunded" : "Cancelled") : booking.PaymentStatus.ToString(),
            ServicePaymentStatus = booking.ServicePaymentStatus.ToString(),
            RoomTypeName = roomTypeNames.Count > 0 ? string.Join(" · ", roomTypeNames) : string.Empty,
            FoodItems = booking.FoodOrders.SelectMany(fo => fo.OrderItems).Select(fi => $"{fi.Quantity}x {fi.MenuItem.Name}").ToList(),
            AmenityItems = booking.BookingAmenities.Select(ba => ba.Amenity.Name).ToList()
        };
        return folio;
    }

    public async Task ProcessPaymentAsync(int bookingId, PaymentRequestDTO dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) throw new KeyNotFoundException("Booking not found.");

        if (booking.PaymentStatus == DAL.Enums.PaymentStatus.Paid)
            throw new InvalidOperationException("Room & amenity payment already completed.");

        var folio = await GenerateFolioAsync(bookingId);
        var bookingAmount = folio.RoomTotal + folio.AmenityTotal;
        if (dto.Amount != bookingAmount)
            throw new InvalidOperationException($"Payment amount ({dto.Amount}) does not match room & amenity total ({bookingAmount}).");

        var receipt = new DAL.Entities.Receipt
        {
            BookingId = bookingId,
            AmountPaid = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            TransactionId = dto.TransactionId,
            PaidAt = DateTime.UtcNow
        };
        await _receiptRepository.AddAsync(receipt);
        await _receiptRepository.SaveChangesAsync();

        booking.PaymentStatus = DAL.Enums.PaymentStatus.Paid;
        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(booking.GuestEmail))
        {
            var updatedFolio = await GenerateFolioAsync(bookingId);
            _ = _emailService.SendPaymentReceiptAsync(updatedFolio, booking.GuestEmail, dto.TransactionId, dto.PaymentMethod);
        }
    }

    public async Task ProcessServicePaymentAsync(int bookingId, PaymentRequestDTO dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) throw new KeyNotFoundException("Booking not found.");

        if (booking.ServicePaymentStatus == DAL.Enums.PaymentStatus.Paid)
            throw new InvalidOperationException("Service payment already completed.");

        var folio = await GenerateFolioAsync(bookingId);
        if (dto.Amount != folio.FoodTotal)
            throw new InvalidOperationException($"Payment amount ({dto.Amount}) does not match food total ({folio.FoodTotal}).");

        var receipt = new DAL.Entities.Receipt
        {
            BookingId = bookingId,
            AmountPaid = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            TransactionId = dto.TransactionId,
            PaidAt = DateTime.UtcNow
        };
        await _receiptRepository.AddAsync(receipt);
        await _receiptRepository.SaveChangesAsync();

        booking.ServicePaymentStatus = DAL.Enums.PaymentStatus.Paid;
        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(booking.GuestEmail))
        {
            var updatedFolio = await GenerateFolioAsync(bookingId);
            _ = _emailService.SendPaymentReceiptAsync(updatedFolio, booking.GuestEmail, dto.TransactionId, dto.PaymentMethod);
        }
    }

    public async Task<object> GetGlobalBillingAsync(string? paymentStatus, DateTime? startDate, DateTime? endDate, string? search, bool detailed, int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        var bookings = await _bookingRepository.GetPaginatedResultAsync(1, int.MaxValue); // We will filter in memory for now or let EF handle it
        var query = bookings.Data.AsQueryable();

        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk");
        if (!isStaff)
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                throw new UnauthorizedAccessException("You must be logged in to view billings.");

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) throw new ArgumentException("User not found.");

            query = query.Where(b => b.UserId == user.Id);
        }

        if (!string.IsNullOrEmpty(paymentStatus) && Enum.TryParse<DAL.Enums.PaymentStatus>(paymentStatus, true, out var statusEnum))
        {
            query = query.Where(b => b.PaymentStatus == statusEnum);
        }

        if (startDate.HasValue) query = query.Where(b => b.CheckInDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.CheckOutDate <= endDate.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.GuestName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     b.BookingRooms.Any(br => br.Room != null && br.Room.RoomNumber.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy.Equals("totalbill", StringComparison.OrdinalIgnoreCase))
            {
                // We must sort by the computed value
                query = sortDescending
                    ? query.OrderByDescending(b => b.BookingRooms.Sum(br => br.LockedInPrice > 0 ? br.LockedInPrice : (br.Room != null && br.Room.RoomType != null ? br.Room.RoomType.BasePrice : 0m)))
                    : query.OrderBy(b => b.BookingRooms.Sum(br => br.LockedInPrice > 0 ? br.LockedInPrice : (br.Room != null && br.Room.RoomType != null ? br.Room.RoomType.BasePrice : 0m)));
            }
            else
            {
                query = query.OrderByDynamic(sortBy, sortDescending);
            }
        }

        var totalCount = query.Count();
        var pagedQuery = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        if (detailed)
        {
            // We need details, so we must fetch full folios (heavy)
            var folios = new List<BillingFolioDTO>();
            foreach (var b in pagedQuery)
            {
                folios.Add(await GenerateFolioAsync(b.Id));
            }
            return new HotelManagement.Repository.Models.PaginatedResult<BillingFolioDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = folios
            };
        }
        else
        {
            var dashboards = pagedQuery.Select(b => new BillingDashboardDTO
            {
                BookingId = b.Id,
                GuestName = b.GuestName,
                // RoomNumber = b.BookingRooms.Any(br => br.Room != null) ? string.Join(", ", b.BookingRooms.Where(br => br.Room != null).Select(br => br.Room!.RoomNumber)) : "Unassigned",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                BookingStatus = b.BookingStatus,
                PaymentStatus = b.BookingStatus == DAL.Enums.BookingStatus.Cancelled ? (b.PaymentStatus == DAL.Enums.PaymentStatus.Paid ? "Refunded" : "Cancelled") : b.PaymentStatus.ToString(),
                BaseRoomTotal = b.BookingRooms.Sum(br => br.LockedInPrice > 0 ? br.LockedInPrice : (br.Room?.RoomType?.BasePrice ?? 0m)) * (b.CheckOutDate - b.CheckInDate).Days
            });
            return new HotelManagement.Repository.Models.PaginatedResult<BillingDashboardDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = dashboards
            };
        }
    }

    public async Task<HotelManagement.Repository.Models.PaginatedResult<ReceiptDTO>> GetReceiptsAsync(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        Func<IQueryable<DAL.Entities.Receipt>, IOrderedQueryable<DAL.Entities.Receipt>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDynamic(sortBy, sortDescending);
        }

        System.Linq.Expressions.Expression<Func<DAL.Entities.Receipt, bool>>? filter = null;
        if (startDate.HasValue && endDate.HasValue)
        {
            filter = r => r.PaidAt >= startDate.Value && r.PaidAt <= endDate.Value;
        }
        else if (startDate.HasValue)
        {
            filter = r => r.PaidAt >= startDate.Value;
        }
        else if (endDate.HasValue)
        {
            filter = r => r.PaidAt <= endDate.Value;
        }

        var records = await _receiptRepository.GetPaginatedResultAsync(pageNumber, pageSize, filter, orderBy);
        var dtos = _mapper.Map<IEnumerable<ReceiptDTO>>(records.Data);

        return new HotelManagement.Repository.Models.PaginatedResult<ReceiptDTO>
        {
            TotalCount = records.TotalCount,
            PageNumber = records.PageNumber,
            PageSize = records.PageSize,
            Data = dtos
        };
    }

    public async Task<ReceiptDTO> GetReceiptByIdAsync(int receiptId)
    {
        var receipt = await _receiptRepository.GetByIdAsync(receiptId);
        if (receipt == null) throw new KeyNotFoundException("Receipt not found.");
        return _mapper.Map<ReceiptDTO>(receipt);
    }
}
