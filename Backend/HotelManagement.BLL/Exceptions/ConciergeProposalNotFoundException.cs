using System;

namespace HotelManagement.BLL.Exceptions;

public class ConciergeProposalNotFoundException : Exception
{
    public string ProposalId { get; }

    public ConciergeProposalNotFoundException() : base("Proposal not found.") 
    { 
        ProposalId = string.Empty; 
    }

    public ConciergeProposalNotFoundException(string proposalId)
        : base($"Proposal {proposalId} not found.")
    {
        ProposalId = proposalId;
    }

    public ConciergeProposalNotFoundException(string message, Exception innerException) 
        : base(message, innerException)
    { 
        ProposalId = string.Empty; 
    }
}