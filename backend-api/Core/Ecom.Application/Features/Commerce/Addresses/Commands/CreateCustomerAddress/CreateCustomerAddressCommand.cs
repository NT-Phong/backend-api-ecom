using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Addresses.Commands.CreateCustomerAddress;
public sealed record CreateCustomerAddressCommand(Guid? AdministrativeAreaId, string RecipientName, string PhoneNumber, string AddressLine, string? PostalCode, string? Label, bool IsDefault) : IRequest<TResult<CustomerAddressDto>>, ITransactionalRequest;
public sealed class CreateCustomerAddressCommandValidator : AbstractValidator<CreateCustomerAddressCommand>
{ public CreateCustomerAddressCommandValidator() { RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200); RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20); RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(500); RuleFor(x => x.PostalCode).MaximumLength(20); RuleFor(x => x.Label).MaximumLength(50); } }
public sealed class CreateCustomerAddressCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<CreateCustomerAddressCommand, TResult<CustomerAddressDto>>
{
    public async Task<TResult<CustomerAddressDto>> Handle(CreateCustomerAddressCommand r, CancellationToken ct)
    {
        if (!current.IsAuthenticated || current.UserId == Guid.Empty) return TResult<CustomerAddressDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var addresses = await uow.Repository<CustomerAddress>().Query().Where(x => x.UserId == current.UserId).ToListAsync(ct);
        var makeDefault = r.IsDefault || addresses.Count == 0;
        if (makeDefault) foreach (var address in addresses.Where(x => x.IsDefault)) { address.SetDefault(false); await uow.Repository<CustomerAddress>().UpdateAsync(address, ct); }
        var entity = CustomerAddress.Create(current.UserId, r.AdministrativeAreaId, r.RecipientName, r.PhoneNumber, r.AddressLine, r.PostalCode, r.Label, makeDefault);
        await uow.Repository<CustomerAddress>().InsertAsync(entity, ct);
        return TResult<CustomerAddressDto>.Success(CustomerAddressMapper.Map(entity));
    }
}
