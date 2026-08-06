using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Addresses.Commands.SetDefaultCustomerAddress;
public sealed record SetDefaultCustomerAddressCommand(Guid AddressId) : IRequest<TResult>, ITransactionalRequest;
public sealed class SetDefaultCustomerAddressCommandValidator : AbstractValidator<SetDefaultCustomerAddressCommand> { public SetDefaultCustomerAddressCommandValidator() => RuleFor(x => x.AddressId).NotEmpty(); }
public sealed class SetDefaultCustomerAddressCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<SetDefaultCustomerAddressCommand, TResult>
{
    public async Task<TResult> Handle(SetDefaultCustomerAddressCommand r, CancellationToken ct)
    {
        if (!current.IsAuthenticated || current.UserId == Guid.Empty) return TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var all = await uow.Repository<CustomerAddress>().Query().Where(x => x.UserId == current.UserId).ToListAsync(ct);
        var selected = all.SingleOrDefault(x => x.Id == r.AddressId); if (selected is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        foreach (var address in all.Where(x => x.IsDefault || x.Id == selected.Id)) { address.SetDefault(address.Id == selected.Id); await uow.Repository<CustomerAddress>().UpdateAsync(address, ct); }
        return TResult.Success();
    }
}
