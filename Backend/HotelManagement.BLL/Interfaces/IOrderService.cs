using HotelManagement.DAL.Enums;
using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IOrderService
{
    Task<PaginatedResult<FoodOrderDTO>> GetAllOrdersAsync(int pageNumber, int pageSize, int? bookingId = null, string? status = null, string? sortBy = null, bool sortDescending = false);
    Task<FoodOrderDTO> GetOrderByIdAsync(int orderId);
    Task<PaginatedResult<FoodOrderDTO>> GetActiveOrdersAsync(int pageNumber, int pageSize, int? bookingId = null, string? sortBy = null, bool sortDescending = false);
    Task UpdateOrderStatusAsync(int orderId, FoodOrderStatus status);
    Task<FoodOrderDTO> CreateOrderAsync(HotelManagement.BLL.DTOs.CreateFoodOrderDTO dto);
}
