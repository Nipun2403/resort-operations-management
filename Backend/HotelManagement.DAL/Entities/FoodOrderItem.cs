namespace HotelManagement.DAL.Entities;
public class FoodOrderItem
{
    public int Id { get; set; }
    public int FoodOrderId { get; set; }
    public FoodOrder FoodOrder { get; set; } = null!;
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
}
