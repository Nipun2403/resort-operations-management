using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Constants;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;

namespace HotelManagement.BLL.Services;

public class StaffService : IStaffService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public StaffService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<StaffResponseDTO>> GetStaffAsync(int pageNumber, int pageSize, bool includeFired = false, string? sortBy = null, bool sortDescending = false, string? searchQuery = null)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0.");
        if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0.");

        // Fetch staff members from the repository
        {
            var staffList = await _userRepository.GetActiveStaffAsync(includeFired);
            var query = staffList.AsQueryable();

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = query.OrderByDynamic(sortBy, sortDescending);
            }
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(st =>
                    st.FirstName.ToLower().Contains(lowerQuery) ||
                    st.LastName.ToLower().Contains(lowerQuery) ||
                    (st.Email != null && st.Email.ToLower().Contains(lowerQuery))
                );
            }

            var totalCount = query.Count();
            var pagedStaff = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var dtos = _mapper.Map<IEnumerable<StaffResponseDTO>>(pagedStaff);
            return new PaginatedResult<StaffResponseDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = dtos
            };
        }
    }

    public async Task<StaffResponseDTO> CreateStaffAsync(StaffRegisterRequestDTO request)
    {
        var validRoles = new[] { UserRoles.Admin, UserRoles.FrontDesk, UserRoles.Kitchen, UserRoles.Housekeeping, UserRoles.Maintenance };
        if (!validRoles.Contains(request.Role))
            throw new ArgumentException($"Invalid role. Must be one of: {string.Join(", ", validRoles)}");

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var staff = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            IsActive = true
        };

        await _userRepository.AddAsync(staff);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<StaffResponseDTO>(staff);
    }

    public async Task<StaffResponseDTO> UpdateStaffAsync(int id, UpdateStaffDTO request)
    {
        var staff = await _userRepository.GetByIdAsync(id);
        if (staff == null || !staff.IsActive) throw new ArgumentException("Active staff member not found.");

        if (staff.Role == "RegisteredUser") throw new ArgumentException("Cannot modify a customer account via the staff endpoint.");

        var validRoles = new[] { UserRoles.Admin, UserRoles.FrontDesk, UserRoles.Kitchen, UserRoles.Housekeeping, UserRoles.Maintenance };
        if (!validRoles.Contains(request.Role))
            throw new ArgumentException($"Invalid role. Must be one of: {string.Join(", ", validRoles)}");

        staff.FirstName = request.FirstName;
        staff.LastName = request.LastName;
        staff.Role = request.Role;

        if (request.IsActive.HasValue)
        {
            staff.IsActive = request.IsActive.Value;
        }

        _userRepository.Update(staff);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<StaffResponseDTO>(staff);
    }

    public async Task DeleteStaffAsync(int id)
    {
        var staff = await _userRepository.GetByIdAsync(id);
        if (staff == null) throw new ArgumentException("Staff member not found.");

        if (staff.Role == "RegisteredUser") throw new ArgumentException("Cannot delete a customer account via the staff endpoint.");

        staff.IsActive = false;
        _userRepository.Update(staff);
        await _userRepository.SaveChangesAsync();
    }
}
