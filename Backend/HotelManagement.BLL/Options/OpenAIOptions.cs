namespace HotelManagement.BLL.Options;

public class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string Endpoint { get; set; } = string.Empty;
}

public class ConciergeOptions
{
    public int MaxConversationTurns { get; set; } = 20;
    public int ConversationTtlHours { get; set; } = 24;
    public int RateLimitPerMinute { get; set; } = 30;
    public int MaxToolCallsPerTurn { get; set; } = 5;
    public int ProposalTtlMinutes { get; set; } = 5;
}