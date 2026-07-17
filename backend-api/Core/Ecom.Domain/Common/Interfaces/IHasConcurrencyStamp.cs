namespace Ecom.Domain.Common.Interfaces
{
    public interface IHasConcurrencyStamp
    {
        Guid ConcurrencyStamp { get; set; }
    }
}

