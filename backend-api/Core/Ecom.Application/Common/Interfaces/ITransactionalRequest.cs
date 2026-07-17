namespace Ecom.Application.Common.Interfaces;

/// <summary>
/// Marks a MediatR request that must run in a database transaction.
/// New starter commands should implement this contract instead of relying on
/// handler-level transaction attributes.
/// </summary>
public interface ITransactionalRequest
{
}

