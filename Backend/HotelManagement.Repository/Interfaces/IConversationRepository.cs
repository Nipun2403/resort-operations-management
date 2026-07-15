using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IConversationRepository
{
    Task<IEnumerable<ConversationMessage>> GetRecentAsync(int userId, string conversationId, int limit);
    Task AddRangeAsync(int userId, string conversationId, IEnumerable<ConversationMessage> messages);
}