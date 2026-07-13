using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class ConciergeActionLogRepository : IConciergeActionLogRepository
{
    private readonly ApplicationDbContext _context;

    public ConciergeActionLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ConciergeActionLog log, CancellationToken ct)
    {
        _context.ConciergeActionLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<ConciergeActionLog>> GetByConversationAsync(int userId, string conversationId)
    {
        return await _context.ConciergeActionLogs
            .Where(l => l.UserId == userId && l.ConversationId == conversationId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();
    }
}