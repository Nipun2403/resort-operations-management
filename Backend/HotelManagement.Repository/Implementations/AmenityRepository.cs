using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;   // for OrderByDynamic
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class AmenityRepository : GenericRepository<Amenity>, IAmenityRepository
{
    public AmenityRepository(ApplicationDbContext context) : base(context) { }

    // NEW IMPLEMENTATION
    public async Task<PaginatedResult<Amenity>> GetPaginatedAmenitiesAsync(
        int pageNumber,
        int pageSize,
        string? searchQuery,
        string? sortBy,
        bool sortDescending)
    {
        var query = _dbSet.AsQueryable();

        // Search filter (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(lowerQuery) ||
                a.Description.ToLower().Contains(lowerQuery)
            );
        }

        // Sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDynamic(sortBy, sortDescending);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Amenity>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = items
        };
    }
}