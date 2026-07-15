using System.Text.Json;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services.Concierge;
using OpenAI.Chat;

namespace HotelManagement.BLL.Services.Concierge;

public static class ToolExecutor
{
    public static async Task<ConciergeActionResultDTO> ExecuteAsync(
        ChatToolCall toolCall, GuestContextDTO ctx, ConciergeService service, CancellationToken ct)
    {
        try
        {
            return toolCall.FunctionName switch
            {
                "CreateFoodOrder" => await service.CreateFoodOrderAsync(
                    JsonSerializer.Deserialize<CreateFoodOrderToolArgs>(toolCall.FunctionArguments)!, ctx, ct),

                "CreateHousekeepingRequest" => await service.CreateHousekeepingRequestAsync(
                    JsonSerializer.Deserialize<CreateHousekeepingToolArgs>(toolCall.FunctionArguments)!, ctx, ct),

                "CreateMaintenanceTicket" => await service.CreateMaintenanceTicketAsync(
                    JsonSerializer.Deserialize<CreateMaintenanceToolArgs>(toolCall.FunctionArguments)!, ctx, ct),

                "GetBookingInfo" => await service.GetBookingInfoAsync(ctx, ct),
                "GetFolioBalance" => await service.GetFolioBalanceAsync(ctx, ct),
                "GetHousekeepingStatus" => await service.GetHousekeepingStatusAsync(ctx, ct),
                "GetMenuItems" => await service.GetMenuItemsAsync(
                    JsonSerializer.Deserialize<GetMenuItemsToolArgs>(toolCall.FunctionArguments) ?? new(), ct),
                "GetActiveOrders" => await service.GetActiveOrdersAsync(ctx, ct),

                _ => new ConciergeActionResultDTO { Success = false, Error = $"Unknown tool: {toolCall.FunctionName}" }
            };
        }
        catch (Exception ex)
        {
            return new ConciergeActionResultDTO { Success = false, Error = ex.Message };
        }
    }
}