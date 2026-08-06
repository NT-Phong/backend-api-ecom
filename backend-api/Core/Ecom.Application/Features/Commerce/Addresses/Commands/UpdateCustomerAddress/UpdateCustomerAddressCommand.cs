using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Addresses.Commands.UpdateCustomerAddress;
public sealed record UpdateCustomerAddressCommand(Guid AddressId, Guid? AdministrativeAreaId, string RecipientName, string PhoneNumber, string AddressLine, string? PostalCode, string? Label) : IRequest<TResult<CustomerAddressDto>>, ITransactionalRequest;
public sealed class UpdateCustomerAddressCommandValidator : AbstractValidator<UpdateCustomerAddressCommand>
{ public UpdateCustomerAddressCommandValidator() { RuleFor(x => x.AddressId).NotEmpty(); RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200); RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20); RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(500); RuleFor(x => x.PostalCode).MaximumLength(20); RuleFor(x => x.Label).MaximumLength(50); } }
public sealed class UpdateCustomerAddressCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<UpdateCustomerAddressCommand, TResult<CustomerAddressDto>>
{
    public async Task<TResult<CustomerAddressDto>> Handle(UpdateCustomerAddressCommand r, CancellationToken ct)
    {
        if (!current.IsAuthenticated || current.UserId == Guid.Empty) return TResult<CustomerAddressDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var entity = await uow.Repository<CustomerAddress>().Query().FirstOrDefaultAsync(x => x.Id == r.AddressId && x.UserId == current.UserId, ct);
        if (entity is null) return TResult<CustomerAddressDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        entity.Update(r.AdministrativeAreaId, r.RecipientName, r.PhoneNumber, r.AddressLine, r.PostalCode, r.Label);
        await uow.Repository<CustomerAddress>().UpdateAsync(entity, ct);
        return TResult<CustomerAddressDto>.Success(CustomerAddressMapper.Map(entity));
    }
}
