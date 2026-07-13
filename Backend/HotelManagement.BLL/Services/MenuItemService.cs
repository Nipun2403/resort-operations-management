using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;

namespace HotelManagement.BLL.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImageUploadService _imageUploadService;

    public MenuItemService(IMenuItemRepository menuItemRepository, IMapper mapper, ICurrentUserService currentUserService, IImageUploadService imageUploadService)
    {
        _menuItemRepository = menuItemRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _imageUploadService = imageUploadService;
    }

    public async Task<PaginatedResult<MenuItemDTO>> GetMenuItemsAsync(
        int pageNumber,
        int pageSize,
        bool? isAvailable = null,
        string? searchQuery = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        // Role‑based default: non‑Admin/Kitchen see only available items
        if (isAvailable.HasValue && !isAvailable.Value && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Kitchen"))
        {
            isAvailable = true;
        }
        else if (!isAvailable.HasValue && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Kitchen"))
        {
            isAvailable = true;
        }

        var pagedResult = await _menuItemRepository.GetPaginatedMenuItemsAsync(
            pageNumber,
            pageSize,
            isAvailable,
            null, // category
            searchQuery,
            sortBy,
            sortDescending);

        var dtos = _mapper.Map<IEnumerable<MenuItemDTO>>(pagedResult.Data);

        return new PaginatedResult<MenuItemDTO>
        {
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            Data = dtos
        };
    }

    public async Task<MenuItemDTO> CreateMenuItemAsync(CreateMenuItemDTO dto)
    {
        var existing = await _menuItemRepository.FindAsync(m => m.Name.ToLower() == dto.Name.ToLower());
        if (existing.Any()) throw new InvalidOperationException($"A menu item with the name '{dto.Name}' already exists in the system.");

        var menuItem = new MenuItem
        {
            Name = dto.Name,
            Price = dto.Price,
            Category = dto.Category,
            IsAvailable = dto.IsAvailable,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl
        };

        await _menuItemRepository.AddAsync(menuItem);
        await _menuItemRepository.SaveChangesAsync();

        if (!string.IsNullOrEmpty(menuItem.ImageUrl))
        {
            await _imageUploadService.AttachToEntityAsync(
                [menuItem.ImageUrl],
                UploadEntityType.MenuItem,
                menuItem.Id,
                _currentUserService.GetUserEmail()!);
        }

        return _mapper.Map<MenuItemDTO>(menuItem);
    }

    public async Task<MenuItemDTO> UpdateMenuItemAsync(int id, MenuItemDTO dto)
    {
        var existingItem = await _menuItemRepository.GetByIdAsync(id);
        if (existingItem == null) throw new ArgumentException("MenuItem not found.");

        existingItem.Name = dto.Name;
        existingItem.Price = dto.Price;
        existingItem.Category = dto.Category;
        existingItem.IsAvailable = dto.IsAvailable;
        existingItem.Description = dto.Description;
        existingItem.ImageUrl = dto.ImageUrl;

        _menuItemRepository.Update(existingItem);
        await _menuItemRepository.SaveChangesAsync();

        if (!string.IsNullOrEmpty(existingItem.ImageUrl))
        {
            await _imageUploadService.AttachToEntityAsync(
                [existingItem.ImageUrl],
                UploadEntityType.MenuItem,
                existingItem.Id,
                _currentUserService.GetUserEmail()!);
        }

        return _mapper.Map<MenuItemDTO>(existingItem);
    }

    public async Task UpdateMenuItemStatusAsync(int id, bool isAvailable)
    {
        var existingItem = await _menuItemRepository.GetByIdAsync(id);
        if (existingItem == null) throw new ArgumentException("MenuItem not found.");

        existingItem.IsAvailable = isAvailable;

        _menuItemRepository.Update(existingItem);
        await _menuItemRepository.SaveChangesAsync();
    }


}
