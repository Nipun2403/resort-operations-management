using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;

namespace HotelManagement.BLL.Services.Concierge;

public class PostgresProposalStore : IProposalStore
{
    private readonly IConciergeProposalRepository _repo;

    public PostgresProposalStore(IConciergeProposalRepository repo)
    {
        _repo = repo;
    }

    public async Task SaveAsync(ConciergeProposalDTO proposal, int userId, string conversationId, CancellationToken ct)
    {
        var entity = new ConciergeProposal
        {
            Id = Guid.Parse(proposal.ProposalId),
            ConversationId = conversationId,
            UserId = userId,
            ToolName = proposal.Action,
            ArgumentsJson = proposal.ArgumentsJson,
            Summary = proposal.Summary,
            Status = "pending",
            ExpiresAt = proposal.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.SaveAsync(entity, userId, conversationId);
    }

    public async Task<List<ConciergeProposalDTO>> GetByIdsAsync(List<string> ids, int userId, string conversationId, CancellationToken ct)
    {
        var guids = ids.Select(Guid.Parse).ToList();
        var entities = await _repo.GetByIdsAsync(guids, userId, conversationId);

        return entities.Select(e => new ConciergeProposalDTO
        {
            ProposalId = e.Id.ToString(),
            Action = e.ToolName,
            Summary = e.Summary,
            ArgumentsJson = e.ArgumentsJson,
            ExpiresAt = e.ExpiresAt
        }).ToList();
    }

    public async Task MarkConfirmedAsync(List<string> ids, int userId, string conversationId, CancellationToken ct)
    {
        await _repo.MarkConfirmedAsync(ids, userId, conversationId);
    }

    public async Task CleanupExpiredAsync(CancellationToken ct)
    {
        await _repo.CleanupExpiredAsync();
    }
}