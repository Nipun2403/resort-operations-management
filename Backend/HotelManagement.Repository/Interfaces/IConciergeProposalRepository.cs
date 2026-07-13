using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IConciergeProposalRepository
{
    Task SaveAsync(ConciergeProposal proposal, int userId, string conversationId);
    Task<List<ConciergeProposal>> GetByIdsAsync(List<Guid> ids, int userId, string conversationId);
    Task MarkConfirmedAsync(List<string> ids, int userId, string conversationId);
    Task CleanupExpiredAsync();
}