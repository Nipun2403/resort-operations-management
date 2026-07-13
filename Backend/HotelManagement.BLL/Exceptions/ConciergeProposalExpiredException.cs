using System;

namespace HotelManagement.BLL.Exceptions;

public class ConciergeProposalExpiredException : Exception
{
    public string ProposalId { get; }

    public ConciergeProposalExpiredException() : base("Proposal has expired.") 
    { 
        ProposalId = string.Empty; 
    }

    public ConciergeProposalExpiredException(string proposalId)
        : base($"Proposal {proposalId} has expired.")
    {
        ProposalId = proposalId;
    }

    public ConciergeProposalExpiredException(string message, Exception innerException) 
        : base(message, innerException)
    { 
        ProposalId = string.Empty; 
    }
}