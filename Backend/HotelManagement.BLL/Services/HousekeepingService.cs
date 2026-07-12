using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.BLL.Services;

public class HousekeepingService : IHousekeepingService
{
    private readonly IHousekeepingRepository _housekeepingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IRoomRepository _roomRepository;

    public HousekeepingService(
        IHousekeepingRepository housekeepingRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IRoomRepository roomRepository)
    {
        _housekeepingRepository = housekeepingRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _roomRepository = roomRepository;
    }

    public async Task CreateCheckoutTriggerAsync(int roomId)
    {
        if (roomId <= 0) throw new ArgumentException("A valid Room ID is required.");
        var roomExists = await _roomRepository.GetByIdAsync(roomId);
        if (roomExists == null) throw new ArgumentException("The specified room does not exist.");

        var task = new Housekeeping { RoomId = roomId, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending };
        await _housekeepingRepository.AddAsync(task);
        await _housekeepingRepository.SaveChangesAsync();
    }
    public async Task CreateGuestTriggerAsync(int roomId, CreateHousekeepingTaskDTO dto)
    {
        if (roomId <= 0) throw new ArgumentException("A valid Room ID is required.");
        var roomExists = await _roomRepository.GetByIdAsync(roomId);
        if (roomExists == null) throw new ArgumentException("The specified room does not exist.");

        if (_currentUserService.IsInRole("RegisteredUser") && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("FrontDesk"))
        {
            var email = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email)) throw new UnauthorizedAccessException("Must be logged in.");

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found.");

            var predicates = new List<System.Linq.Expressions.Expression<Func<Booking, bool>>> { b => b.UserId == user.Id };
            var paginatedBookings = await _bookingRepository.GetPaginatedBookingsWithDetailsAsync(1, 100, predicates);
            var userBookings = paginatedBookings.Data;
            var isActiveInRoom = userBookings.Any(b => b.BookingRooms.Any(br => br.RoomId == roomId) && b.BookingStatus == BookingStatus.CheckedIn);

            if (!isActiveInRoom)
                throw new ArgumentException("You do not have an active booking for this room.");
        }

        var existingTasks = await _housekeepingRepository.FindAsync(h => h.RoomId == roomId && h.Description == dto.Description && h.Status != HousekeepingStatus.Completed);
        if (existingTasks.Any())
            throw new InvalidOperationException("An identical housekeeping task is already pending or in progress for this room.");

        var task = new Housekeeping 
        { 
            RoomId = roomId, 
            Location = $"Room {roomExists.RoomNumber}",
            OriginType = HousekeepingOriginType.GuestRequested, 
            Status = HousekeepingStatus.Pending,
            Description = dto.Description,
            IsEmergency = dto.IsEmergency
        };
        await _housekeepingRepository.AddAsync(task);
        await _housekeepingRepository.SaveChangesAsync();

        await _notificationService.SendHousekeepingAlertAsync($"New Housekeeping Ticket\nLocation : Room {roomId}\nDescription : {dto.Description}");
    }

    public async Task CreateInternalTriggerAsync(CreateInternalHousekeepingTaskDTO dto)
    {
        var existingTasks = await _housekeepingRepository.FindAsync(h => h.Location == dto.Location && h.Description == dto.Description && h.Status != HousekeepingStatus.Completed);
        if (existingTasks.Any())
            throw new InvalidOperationException("An identical internal housekeeping task is already pending or in progress for this location.");
        var task = new Housekeeping 
        { 
            RoomId = null, 
            Location = dto.Location,
            Description = dto.Description,
            OriginType = HousekeepingOriginType.StaffRequested, 
            Status = HousekeepingStatus.Pending,
            IsEmergency = dto.IsEmergency
        };
        
        await _housekeepingRepository.AddAsync(task);
        await _housekeepingRepository.SaveChangesAsync();

        await _notificationService.SendHousekeepingAlertAsync($"New Housekeeping Ticket\nLocation : 000 (Internal) - {dto.Location}\nDescription : {dto.Description}");
    }

    public async Task UpdateStatusAsync(int taskId, HousekeepingStatus status)
    {
        var task = await _housekeepingRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException("Housekeeping task not found.");

        if (status == HousekeepingStatus.InProgress && task.Status != HousekeepingStatus.InProgress)
        {
            var email = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email)) throw new UnauthorizedAccessException("Must be logged in.");
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found.");

            var activeTasks = await _housekeepingRepository.FindAsync(h =>
                h.AssignedToUserId == user.Id && h.Status == HousekeepingStatus.InProgress);
            if (activeTasks.Count() >= 2)
                throw new InvalidOperationException("You can only work on up to 2 tasks at a time. Complete an existing task before starting a new one.");

            task.AssignedToUserId = user.Id;
        }

        if (status == HousekeepingStatus.Completed && task.Status == HousekeepingStatus.InProgress)
        {
            var email = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email)) throw new UnauthorizedAccessException("Must be logged in.");
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found.");
            if (task.AssignedToUserId != user.Id)
                throw new UnauthorizedAccessException("You can only edit your own assigned tasks.");
            task.AssignedToUserId = null;
        }

        task.Status = status;
        if (status == HousekeepingStatus.InProgress) task.StartedAt = DateTime.UtcNow;
        if (status == HousekeepingStatus.Completed) task.FinishedAt = DateTime.UtcNow;
        try
        {
            _housekeepingRepository.Update(task);
            await _housekeepingRepository.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("This task was just claimed by another staff member. Please refresh and try again.");
        }
    }

    public async Task<PaginatedResult<HousekeepingDTO>> GetAllAsync(int pageNumber, int pageSize, string? status = null, string? sortBy = null, bool sortDescending = false, bool assignedToMe = false)
    {
        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk") || _currentUserService.IsInRole("Housekeeping");
        List<int> userRoomIds = new List<int>();
        if (!isStaff)
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail)) throw new UnauthorizedAccessException("Must be logged in.");
            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) throw new ArgumentException("User not found.");

            var paginatedBookings = await _bookingRepository.GetPaginatedBookingsWithDetailsAsync(1, 100, new List<System.Linq.Expressions.Expression<Func<Booking, bool>>> { b => b.UserId == user.Id });
            userRoomIds = paginatedBookings.Data.SelectMany(b => b.BookingRooms).Where(br => br.RoomId.HasValue).Select(br => br.RoomId!.Value).ToList();
        }

        Func<IQueryable<Housekeeping>, IOrderedQueryable<Housekeeping>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenByDynamic(sortBy, sortDescending);
        }
        else
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenBy(t => t.CreatedAt);
        }

        System.Linq.Expressions.Expression<Func<Housekeeping, bool>>? filter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<HousekeepingStatus>(status, true, out var parsedStatus))
        {
            if (!isStaff)
                filter = h => h.RoomId.HasValue && userRoomIds.Contains(h.RoomId.Value) && h.Status == parsedStatus;
            else
                filter = h => h.Status == parsedStatus;
        }
        else
        {
            if (!isStaff)
                filter = h => h.RoomId.HasValue && userRoomIds.Contains(h.RoomId.Value);
        }

        if (assignedToMe && isStaff)
        {
            var email = _currentUserService.GetUserEmail();
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                    filter = h => h.AssignedToUserId == user.Id && h.Status == HousekeepingStatus.InProgress;
            }
        }

        var records = await _housekeepingRepository.GetPaginatedResultAsync(pageNumber, pageSize, filter, orderBy);
        var dtos = _mapper.Map<IEnumerable<HousekeepingDTO>>(records.Data);
        return new PaginatedResult<HousekeepingDTO>
        {
            TotalCount = records.TotalCount,
            PageNumber = records.PageNumber,
            PageSize = records.PageSize,
            Data = dtos
        };
    }

    public async Task<PaginatedResult<HousekeepingDTO>> GetActiveAsync(int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk") || _currentUserService.IsInRole("Housekeeping");
        List<int> userRoomIds = new List<int>();
        if (!isStaff)
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail)) throw new UnauthorizedAccessException("Must be logged in.");
            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) throw new ArgumentException("User not found.");

            var paginatedBookings = await _bookingRepository.GetPaginatedBookingsWithDetailsAsync(1, 100, new List<System.Linq.Expressions.Expression<Func<Booking, bool>>> { b => b.UserId == user.Id });
            userRoomIds = paginatedBookings.Data.SelectMany(b => b.BookingRooms).Where(br => br.RoomId.HasValue).Select(br => br.RoomId!.Value).ToList();
        }

        Func<IQueryable<Housekeeping>, IOrderedQueryable<Housekeeping>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenByDynamic(sortBy, sortDescending);
        }
        else
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenBy(t => t.CreatedAt);
        }

        System.Linq.Expressions.Expression<Func<Housekeeping, bool>>? filter = null;
        if (!isStaff)
            filter = h => h.RoomId.HasValue && userRoomIds.Contains(h.RoomId.Value) && h.Status != HousekeepingStatus.Completed;
        else
            filter = h => h.Status != HousekeepingStatus.Completed;

        var records = await _housekeepingRepository.GetPaginatedResultAsync(pageNumber, pageSize, filter, orderBy);
        var dtos = _mapper.Map<IEnumerable<HousekeepingDTO>>(records.Data);
        return new PaginatedResult<HousekeepingDTO>
        {
            TotalCount = records.TotalCount,
            PageNumber = records.PageNumber,
            PageSize = records.PageSize,
            Data = dtos
        };
    }
}
