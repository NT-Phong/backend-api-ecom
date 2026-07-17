using Ecom.Domain.Constants;

namespace Ecom.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public ErrorCodes? ErrorCode { get; }

    protected DomainException(string message, ErrorCodes? errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
} 
