using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class ConversationRepository : IConversationRepository
{
    private readonly ApplicationDbContext _context;

    public ConversationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConversationMessage>> GetRecentAsync(int userId, string conversationId, int limit)
    {
        return await _context.ConversationMessages
            .Where(m => m.UserId == userId && m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task AddRangeAsync(int userId, string conversationId, IEnumerable<ConversationMessage> messages)
    {
        _context.ConversationMessages.AddRange(messages);
        await _context.SaveChangesAsync();
    }
}