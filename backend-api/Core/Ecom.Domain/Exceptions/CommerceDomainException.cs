namespace Ecom.Domain.Exceptions;

public sealed class CommerceDomainException : DomainException
{
    public string Code { get; }

    public CommerceDomainException(string code, string message)
        : base(message, null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("A commerce error code is required.", nameof(code));

        Code = code;
    }
}
