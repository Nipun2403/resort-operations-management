using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Interfaces;

namespace HotelManagement.BLL.Services.Concierge;

public class PostgresConversationStore : IConversationStore
{
    private readonly IConversationRepository _repo;

    public PostgresConversationStore(IConversationRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ConversationTurn>> GetAsync(string scopedKey, int userId, CancellationToken ct)
    {
        var messages = await _repo.GetRecentAsync(userId, scopedKey, 8);
        var messageList = messages.ToList();
        var turns = new List<ConversationTurn>();

        for (int i = 0; i < messageList.Count - 1; i += 2)
        {
            if (messageList[i].Role == "user" && messageList[i + 1].Role == "assistant")
            {
                turns.Add(new ConversationTurn
                {
                    UserMessage = messageList[i].Content,
                    AssistantReply = messageList[i + 1].Content,
                    Timestamp = messageList[i].CreatedAt
                });
            }
        }

        return turns;
    }

    public async Task AppendAsync(string scopedKey, int userId, string userMsg, string assistantMsg, CancellationToken ct)
    {
        var messages = new[]
        {
            new HotelManagement.DAL.Entities.ConversationMessage
            {
                UserId = userId,
                ConversationId = scopedKey,
                Role = "user",
                Content = userMsg,
                CreatedAt = DateTime.UtcNow
            },
            new HotelManagement.DAL.Entities.ConversationMessage
            {
                UserId = userId,
                ConversationId = scopedKey,
                Role = "assistant",
                Content = assistantMsg,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _repo.AddRangeAsync(userId, scopedKey, messages);
    }
}