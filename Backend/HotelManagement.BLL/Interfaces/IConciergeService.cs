using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IConciergeService
{
    Task<ConciergeChatResponseDTO> ProcessMessageAsync(string userMessage, string? conversationId = null, CancellationToken ct = default);
    Task<ConciergeChatResponseDTO> ConfirmProposalsAsync(string conversationId, List<string> proposalIds, CancellationToken ct = default);
    Task<List<ConciergeProposalDTO>> GetPendingProposalsAsync(string conversationId, CancellationToken ct = default);
    Task<GuestContextDTO> GetGuestContextAsync(CancellationToken ct = default);
}

public interface IConversationStore
{
    Task<List<ConversationTurn>> GetAsync(string scopedKey, int userId, CancellationToken ct);
    Task AppendAsync(string scopedKey, int userId, string userMsg, string assistantMsg, CancellationToken ct);
}

public interface IProposalStore
{
    Task SaveAsync(ConciergeProposalDTO proposal, int userId, string conversationId, CancellationToken ct);
    Task<List<ConciergeProposalDTO>> GetByIdsAsync(List<string> ids, int userId, string conversationId, CancellationToken ct);
    Task MarkConfirmedAsync(List<string> ids, int userId, string conversationId, CancellationToken ct);
    Task CleanupExpiredAsync(CancellationToken ct);
}