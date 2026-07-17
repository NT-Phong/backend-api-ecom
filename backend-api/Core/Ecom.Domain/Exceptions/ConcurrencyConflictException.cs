namespace Ecom.Domain.Exceptions;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("The data was changed by another request. Reload it and try again.")
    {
    }
}

