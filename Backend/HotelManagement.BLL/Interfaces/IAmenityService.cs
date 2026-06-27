using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IAmenityService
{
    Task<PaginatedResult<AmenityDTO>> GetAllAmenitiesAsync(
    int pageNumber,
    int pageSize,
    string? searchQuery = null,
    string? sortBy = null,
    bool sortDescending = false);
    Task<AmenityDTO?> GetAmenityByIdAsync(int id);
    Task<AmenityDTO> CreateAmenityAsync(CreateUpdateAmenityDTO dto);
    Task UpdateAmenityAsync(int id, CreateUpdateAmenityDTO dto, bool isAvailable);
    // Task UpdateAmenityStatusAsync(int id, bool isAvailable);


    Task SubscribeAsync(int bookingId, int amenityId);
}
