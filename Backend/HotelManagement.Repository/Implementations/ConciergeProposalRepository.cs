using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class ConciergeProposalRepository : IConciergeProposalRepository
{
    private readonly ApplicationDbContext _context;

    public ConciergeProposalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(ConciergeProposal proposal, int userId, string conversationId)
    {
        proposal.UserId = userId;
        proposal.ConversationId = conversationId;
        _context.ConciergeProposals.Add(proposal);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ConciergeProposal>> GetByIdsAsync(List<Guid> ids, int userId, string conversationId)
    {
        var query = _context.ConciergeProposals
            .Where(p => p.UserId == userId && p.ConversationId == conversationId);

        if (ids == null || ids.Count == 0)
        {
            query = query.Where(p => p.Status == "pending" && p.ExpiresAt > DateTime.UtcNow);
        }
        else
        {
            query = query.Where(p => ids.Contains(p.Id));
        }

        return await query.ToListAsync();
    }

    public async Task MarkConfirmedAsync(List<string> ids, int userId, string conversationId)
    {
        var guids = ids.Select(Guid.Parse).ToList();
        var proposals = await _context.ConciergeProposals
            .Where(p => p.UserId == userId && p.ConversationId == conversationId && guids.Contains(p.Id))
            .ToListAsync();

        foreach (var p in proposals)
        {
            p.Status = "confirmed";
            p.ConfirmedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken ct = default)
    {
        var expired = await _context.ConciergeProposals
            .Where(p => p.Status == "pending" && p.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var p in expired)
        {
            p.Status = "expired";
        }
        await _context.SaveChangesAsync(ct);
        return expired.Count;
    }
}