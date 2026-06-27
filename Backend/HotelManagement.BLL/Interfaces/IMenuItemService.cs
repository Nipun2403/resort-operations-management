using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IMenuItemService
{
    Task<PaginatedResult<MenuItemDTO>> GetMenuItemsAsync(
    int pageNumber,
    int pageSize,
    bool isAvailable = true,
    string? searchQuery = null,
    string? sortBy = null,
    bool sortDescending = false);

    Task<MenuItemDTO> CreateMenuItemAsync(CreateMenuItemDTO dto);
    Task<MenuItemDTO> UpdateMenuItemAsync(int id, MenuItemDTO dto);
    Task UpdateMenuItemStatusAsync(int id, bool isAvailable);
    // Task DeleteMenuItemAsync(int id);
}
