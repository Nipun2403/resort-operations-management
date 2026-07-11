using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using System.Text.RegularExpressions;
using HotelManagement.Repository.Utilities;

namespace HotelManagement.BLL.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IRoomRepository _roomRepository;

    public MaintenanceService(
        IMaintenanceRepository maintenanceRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IRoomRepository roomRepository)
    {
        _maintenanceRepository = maintenanceRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _roomRepository = roomRepository;
    }

    public async Task<PaginatedResult<MaintenanceTaskDTO>> GetAllTasksAsync(int pageNumber, int pageSize, string? status = null, string? sortBy = null, bool sortDescending = false, bool assignedToMe = false)
    {
        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk") || _currentUserService.IsInRole("Housekeeping") || _currentUserService.IsInRole("Maintenance");
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

        Func<IQueryable<MaintenanceTask>, IOrderedQueryable<MaintenanceTask>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenByDynamic(sortBy, sortDescending);
        }
        else
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenBy(t => t.CreatedAt);
        }

        System.Linq.Expressions.Expression<Func<MaintenanceTask, bool>>? filter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MaintenanceStatus>(status, true, out var parsedStatus))
        {
            if (!isStaff)
                filter = m => m.RoomId.HasValue && userRoomIds.Contains(m.RoomId.Value) && m.Status == parsedStatus;
            else
                filter = m => m.Status == parsedStatus;
        }
        else
        {
            if (!isStaff)
                filter = m => m.RoomId.HasValue && userRoomIds.Contains(m.RoomId.Value);
        }

        if (assignedToMe && isStaff)
        {
            var email = _currentUserService.GetUserEmail();
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                    filter = m => m.AssignedToUserId == user.Id && m.Status == MaintenanceStatus.InProgress;
            }
        }

        var records = await _maintenanceRepository.GetPaginatedResultAsync(pageNumber, pageSize, filter, orderBy);
        var dtos = _mapper.Map<IEnumerable<MaintenanceTaskDTO>>(records.Data);
        
        return new PaginatedResult<MaintenanceTaskDTO>
        {
            TotalCount = records.TotalCount,
            PageNumber = records.PageNumber,
            PageSize = records.PageSize,
            Data = dtos
        };
    }

    public async Task<PaginatedResult<MaintenanceTaskDTO>> GetActiveTasksAsync(int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk") || _currentUserService.IsInRole("Housekeeping") || _currentUserService.IsInRole("Maintenance");
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

        Func<IQueryable<MaintenanceTask>, IOrderedQueryable<MaintenanceTask>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenByDynamic(sortBy, sortDescending);
        }
        else
        {
            orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenBy(t => t.CreatedAt);
        }

        System.Linq.Expressions.Expression<Func<MaintenanceTask, bool>>? filter = null;
        if (!isStaff)
            filter = m => m.RoomId.HasValue && userRoomIds.Contains(m.RoomId.Value) && m.Status != MaintenanceStatus.Completed;
        else
            filter = m => m.Status != MaintenanceStatus.Completed;

        var records = await _maintenanceRepository.GetPaginatedResultAsync(pageNumber, pageSize, filter, orderBy);
        var dtos = _mapper.Map<IEnumerable<MaintenanceTaskDTO>>(records.Data);
        
        return new PaginatedResult<MaintenanceTaskDTO>
        {
            TotalCount = records.TotalCount,
            PageNumber = records.PageNumber,
            PageSize = records.PageSize,
            Data = dtos
        };
    }

    public async Task<MaintenanceTaskDTO> CreateTicketAsync(int roomId, CreateMaintenanceTaskDTO dto, string? originTypeOverride = null)
    {
        // Validation: Description must not be just numbers/symbols
        if (!Regex.IsMatch(dto.Description, @"[a-zA-Z]"))
        {
            throw new ArgumentException("Description must contain at least one alphabet character.");
        }

        if (roomId <= 0)
        {
            throw new ArgumentException("A valid Room ID is required.");
        }

        var roomExists = await _roomRepository.GetByIdAsync(roomId);
        if (roomExists == null)
        {
            throw new ArgumentException("The specified room does not exist.");
        }

        var originType = MaintenanceOriginType.SystemAutomated; // default
        
        if (originTypeOverride != null && Enum.TryParse<MaintenanceOriginType>(originTypeOverride, out var parsedOrigin))
        {
            originType = parsedOrigin;
        }
        else if (_currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk") || _currentUserService.IsInRole("Housekeeping") || _currentUserService.IsInRole("Maintenance"))
        {
            originType = MaintenanceOriginType.StaffRequested;
        }
        else if (_currentUserService.IsInRole("RegisteredUser"))
        {
            originType = MaintenanceOriginType.GuestRequested;
            
            // Validate the guest owns the room
            var email = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email)) throw new UnauthorizedAccessException("Must be logged in.");

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found.");

            var predicates = new List<System.Linq.Expressions.Expression<Func<Booking, bool>>> { b => b.UserId == user.Id };
            var paginatedBookings = await _bookingRepository.GetPaginatedBookingsWithDetailsAsync(1, 100, predicates);
            var userBookings = paginatedBookings.Data;
            var isActiveInRoom = userBookings.Any(b => b.BookingRooms.Any(br => br.RoomId == roomId) && b.BookingStatus == BookingStatus.CheckedIn);

            if (!isActiveInRoom)
                throw new ArgumentException("You do not have an active booking for this room to request maintenance.");
        }

        var existingTasks = await _maintenanceRepository.FindAsync(m => m.RoomId == roomId && m.Description == dto.Description && m.Status != MaintenanceStatus.Completed);
        if (existingTasks.Any())
            throw new InvalidOperationException("An identical maintenance ticket is already pending or in progress for this room.");

        var task = new MaintenanceTask
        {
            RoomId = roomId,
            Location = $"Room {roomExists.RoomNumber}",
            OriginType = originType,
            Status = MaintenanceStatus.Pending,
            Description = dto.Description,
            IsEmergency = dto.IsEmergency
        };

        await _maintenanceRepository.AddAsync(task);
        await _maintenanceRepository.SaveChangesAsync();

        // Broadcast alert
        await _notificationService.SendMaintenanceAlertAsync($"New Maintenance Ticket\nLocation : Room {roomId}\nDescription : {dto.Description}");

        return _mapper.Map<MaintenanceTaskDTO>(task);
    }

    public async Task<MaintenanceTaskDTO> CreateInternalTicketAsync(CreateInternalMaintenanceTaskDTO dto)
    {
        var existingTasks = await _maintenanceRepository.FindAsync(m => m.Location == dto.Location && m.Description == dto.Description && m.Status != MaintenanceStatus.Completed);
        if (existingTasks.Any())
            throw new InvalidOperationException("An identical internal maintenance ticket is already pending or in progress for this location.");

        var task = new MaintenanceTask
        {
            RoomId = null,
            Location = dto.Location,
            OriginType = MaintenanceOriginType.StaffRequested,
            Status = MaintenanceStatus.Pending,
            Description = dto.Description,
            IsEmergency = dto.IsEmergency
        };

        await _maintenanceRepository.AddAsync(task);
        await _maintenanceRepository.SaveChangesAsync();

        await _notificationService.SendMaintenanceAlertAsync($"New Maintenance Ticket\nLocation : 000 (Internal) - {dto.Location}\nDescription : {dto.Description}");

        return _mapper.Map<MaintenanceTaskDTO>(task);
    }

    public async Task<MaintenanceTaskDTO> UpdateStatusAsync(int id, UpdateMaintenanceStatusDTO dto)
    {
        var task = await _maintenanceRepository.GetByIdAsync(id);
        if (task == null) throw new ArgumentException("Maintenance task not found.");

        if (dto.Status == MaintenanceStatus.InProgress && task.Status != MaintenanceStatus.InProgress)
        {
            var email = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email)) throw new UnauthorizedAccessException("Must be logged in.");
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new ArgumentException("User not found.");

            var activeTasks = await _maintenanceRepository.FindAsync(m =>
                m.AssignedToUserId == user.Id && m.Status == MaintenanceStatus.InProgress);
            if (activeTasks.Count() >= 2)
                throw new InvalidOperationException("You can only work on up to 2 tasks at a time. Complete an existing task before starting a new one.");

            task.AssignedToUserId = user.Id;
        }

        if (dto.Status == MaintenanceStatus.Completed && task.Status == MaintenanceStatus.InProgress)
            task.AssignedToUserId = null;

        task.Status = dto.Status;
        if (dto.Status == MaintenanceStatus.InProgress) task.StartedAt = DateTime.UtcNow;
        if (dto.Status == MaintenanceStatus.Completed) task.FinishedAt = DateTime.UtcNow;

        _maintenanceRepository.Update(task);
        await _maintenanceRepository.SaveChangesAsync();

        return _mapper.Map<MaintenanceTaskDTO>(task);
    }
}
