using System.Text.RegularExpressions;

namespace HotelManagement.BLL.Services.Concierge;

public static class InputSanitizer
{
    private static readonly string[] BlockedPatterns =
    {
        @"(?i)ignore\s+(all\s+)?previous\s+instructions",
        @"(?i)system\s*:",
        @"(?i)assistant\s*:",
        @"(?i)you\s+are\s+(not\s+)?(a\s+)?(concierge|assistant)",
        @"(?i)forget\s+(everything|all)",
        @"(?i)developer\s*mode",
        @"(?i)jailbreak",
    };

    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var result = input;
        foreach (var pattern in BlockedPatterns)
        {
            result = Regex.Replace(result, pattern, "[REDACTED]", RegexOptions.Multiline);
        }

        return result.Trim();
    }
}