using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Addresses.Commands.DeleteCustomerAddress;
public sealed record DeleteCustomerAddressCommand(Guid AddressId) : IRequest<TResult>, ITransactionalRequest;
public sealed class DeleteCustomerAddressCommandValidator : AbstractValidator<DeleteCustomerAddressCommand> { public DeleteCustomerAddressCommandValidator() => RuleFor(x => x.AddressId).NotEmpty(); }
public sealed class DeleteCustomerAddressCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<DeleteCustomerAddressCommand, TResult>
{
    public async Task<TResult> Handle(DeleteCustomerAddressCommand r, CancellationToken ct)
    {
        if (!current.IsAuthenticated || current.UserId == Guid.Empty) return TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var entity = await uow.Repository<CustomerAddress>().Query().FirstOrDefaultAsync(x => x.Id == r.AddressId && x.UserId == current.UserId, ct);
        if (entity is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        await uow.Repository<CustomerAddress>().DeleteAsync(entity, ct); return TResult.Success();
    }
}
