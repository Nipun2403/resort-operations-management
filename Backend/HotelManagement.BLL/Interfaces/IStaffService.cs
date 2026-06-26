using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IStaffService
{
    Task<PaginatedResult<StaffResponseDTO>> GetStaffAsync(int pageNumber, int pageSize, bool includeFired = false, string? sortBy = null, bool sortDescending = false);
    Task<StaffResponseDTO> CreateStaffAsync(StaffRegisterRequestDTO dto);
    Task<StaffResponseDTO> UpdateStaffAsync(int id, UpdateStaffDTO dto);
    Task DeleteStaffAsync(int id);
}
