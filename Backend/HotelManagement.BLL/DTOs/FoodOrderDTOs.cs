using System.ComponentModel.DataAnnotations;
using HotelManagement.DAL.Enums;

namespace HotelManagement.BLL.DTOs;

public class FoodOrderDTO
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string GeneratedAt { get; set; } = string.Empty;
    public string? FinishedAt { get; set; }
    public FoodOrderStatus OrderStatus { get; set; }
    public List<FoodOrderItemDTO> OrderItems { get; set; } = new();
}

public class FoodOrderItemDTO
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
}

public class CreateFoodOrderDTO
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required, MinLength(1, ErrorMessage = "At least one item must be ordered.")]
    public List<CreateFoodOrderItemDTO> Items { get; set; } = new();
}

public class CreateFoodOrderItemDTO
{
    [Required]
    public int MenuItemId { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}

public class UpdateOrderStatusDTO
{
    public FoodOrderStatus Status { get; set; }
}
