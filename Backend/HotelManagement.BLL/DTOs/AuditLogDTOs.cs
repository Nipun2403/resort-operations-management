using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace HotelManagement.BLL.DTOs;

public class AuditLogDTO
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    [JsonPropertyName("recordId")]
    public JsonDocument? PrimaryKey { get; set; }
    public JsonDocument? OldValues { get; set; }
    public JsonDocument? NewValues { get; set; }
    public string? ChangedByEmail { get; set; }
    public string? ChangedByName { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
