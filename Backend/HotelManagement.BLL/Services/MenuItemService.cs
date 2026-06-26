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

    public MenuItemService(IMenuItemRepository menuItemRepository, IMapper mapper, ICurrentUserService currentUserService)
    {
        _menuItemRepository = menuItemRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<MenuItemDTO>> GetMenuItemsAsync(int pageNumber, int pageSize, bool? isAvailable = null, string? sortBy = null, bool sortDescending = false)
    {
        if (isAvailable.HasValue && !isAvailable.Value && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Kitchen"))
        {
            isAvailable = true;
        }
        else if (!isAvailable.HasValue && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Kitchen"))
        {
            isAvailable = true;
        }

        var allItems = await _menuItemRepository.GetAllAsync();
        var query = allItems.AsQueryable();

        if (isAvailable.HasValue)
        {
            query = query.Where(m => m.IsAvailable == isAvailable.Value);
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDynamic(sortBy, sortDescending);
        }

        var totalCount = query.Count();
        var pagedItems = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var dtos = _mapper.Map<IEnumerable<MenuItemDTO>>(pagedItems);
        return new PaginatedResult<MenuItemDTO>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
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
            IsAvailable = dto.IsAvailable
        };

        await _menuItemRepository.AddAsync(menuItem);
        await _menuItemRepository.SaveChangesAsync();

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

        _menuItemRepository.Update(existingItem);
        await _menuItemRepository.SaveChangesAsync();

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

    // public async Task DeleteMenuItemAsync(int id)
    // {
    //     var existingItem = await _menuItemRepository.GetByIdAsync(id);
    //     if (existingItem == null) throw new ArgumentException("MenuItem not found.");

    //     existingItem.IsAvailable = false;
    //     _menuItemRepository.Update(existingItem);
    //     await _menuItemRepository.SaveChangesAsync();
    // }
}
