using System.Text.Json.Serialization;

namespace HotelManagement.BLL.Services.Concierge;

public class CreateFoodOrderToolArgs
{
    [JsonPropertyName("items")]
    public List<FoodOrderItemToolArg> Items { get; set; } = new();
}

public class FoodOrderItemToolArg
{
    [JsonPropertyName("menuItemId")] public int MenuItemId { get; set; }
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
}

public class CreateHousekeepingToolArgs
{
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("isEmergency")] public bool IsEmergency { get; set; } = false;
}

public class CreateMaintenanceToolArgs
{
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("isEmergency")] public bool IsEmergency { get; set; } = false;
}

public class GetMenuItemsToolArgs
{
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("search")] public string? Search { get; set; }
    [JsonPropertyName("availableOnly")] public bool AvailableOnly { get; set; } = true;
}