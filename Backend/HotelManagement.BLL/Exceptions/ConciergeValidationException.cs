using System;

namespace HotelManagement.BLL.Exceptions;

public class ConciergeValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ConciergeValidationException() : base("Validation failed.") 
    { 
        Errors = new Dictionary<string, string[]>();
    }

    public ConciergeValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ConciergeValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ConciergeValidationException(string message, Exception innerException) 
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }
}