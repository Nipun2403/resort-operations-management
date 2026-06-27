using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IAmenityRepository : IGenericRepository<Amenity>
{
  // NEW: Paginated query with search and sorting
  Task<HotelManagement.Repository.Models.PaginatedResult<Amenity>> GetPaginatedAmenitiesAsync(
      int pageNumber,
      int pageSize,
      string? searchQuery,
      string? sortBy,
      bool sortDescending,
      bool isAvailable
      );
}