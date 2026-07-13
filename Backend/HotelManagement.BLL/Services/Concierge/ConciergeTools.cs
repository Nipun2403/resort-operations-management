using System.Text.Json;
using OpenAI.Chat;

namespace HotelManagement.BLL.Services.Concierge;

public static class ConciergeTools
{
    public static readonly List<ChatTool> Definitions = new()
    {
        ChatTool.CreateFunctionTool(
            functionName: "CreateFoodOrder",
            functionDescription: "Place a room-service order for the guest's active booking. Always confirm items & quantities with guest before calling.",
            functionParameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    items = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                menuItemId = new { type = "integer" },
                                quantity = new { type = "integer", minimum = 1, maximum = 20 }
                            },
                            required = new[] { "menuItemId", "quantity" }
                        }
                    }
                },
                required = new[] { "items" }
            })
        ),

        ChatTool.CreateFunctionTool(
            functionName: "CreateHousekeepingRequest",
            functionDescription: "Request housekeeping (extra towels, cleaning, amenities)",
            functionParameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    description = new { type = "string", maxLength = 500 },
                    isEmergency = new { type = "boolean" }
                },
                required = new[] { "description" }
            })
        ),

        ChatTool.CreateFunctionTool(
            functionName: "CreateMaintenanceTicket",
            functionDescription: "Report a maintenance issue in the guest's room",
            functionParameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    description = new { type = "string", maxLength = 500 },
                    isEmergency = new { type = "boolean" }
                },
                required = new[] { "description" }
            })
        ),

        ChatTool.CreateFunctionTool(
            functionName: "GetBookingInfo",
            functionDescription: "Retrieve current booking details (check-in/out, room, status)",
            functionParameters: BinaryData.FromObjectAsJson(new { type = "object", properties = new { } })
        ),

        ChatTool.CreateFunctionTool(
            functionName: "GetFolioBalance",
            functionDescription: "Get current folio/billing balance for the stay",
            functionParameters: BinaryData.FromObjectAsJson(new { type = "object", properties = new { } })
        ),

        ChatTool.CreateFunctionTool(
            functionName: "GetHousekeepingStatus",
            functionDescription: "Check if room has been cleaned / status of housekeeping requests",
            functionParameters: BinaryData.FromObjectAsJson(new { type = "object", properties = new { } })
        ),

        ChatTool.CreateFunctionTool(
            functionName: "GetMenuItems",
            functionDescription: "Browse available menu items (filter by category, availability)",
            functionParameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    category = new { type = "string" },
                    search = new { type = "string" },
                    availableOnly = new { type = "boolean" }
                }
            })
        ),

        ChatTool.CreateFunctionTool(
            functionName: "GetActiveOrders",
            functionDescription: "List guest's current/pending room-service orders",
            functionParameters: BinaryData.FromObjectAsJson(new { type = "object", properties = new { } })
        )
    };

    public static readonly HashSet<string> SideEffectToolNames = new()
    {
        "CreateFoodOrder", "CreateHousekeepingRequest", "CreateMaintenanceTicket"
    };

    public const int MaxToolCallsPerTurn = 5;

    public static List<ChatTool> BuildChatTools() => Definitions;
}