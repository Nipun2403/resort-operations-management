using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IConciergeActionLogRepository
{
    Task AddAsync(ConciergeActionLog log, CancellationToken ct);
    Task<IEnumerable<ConciergeActionLog>> GetByConversationAsync(int userId, string conversationId);
}