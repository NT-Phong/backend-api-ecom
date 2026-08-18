using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Commands.UpdateStockLocation;

public sealed record UpdateStockLocationCommand(Guid StockLocationId, Guid ConcurrencyStamp, string Name, Guid? AdministrativeAreaId, string? AddressLine, bool IsActive)
    : IRequest<TResult<StockLocationDto>>, ITransactionalRequest;
public sealed class UpdateStockLocationCommandValidator : AbstractValidator<UpdateStockLocationCommand>
{ public UpdateStockLocationCommandValidator() { RuleFor(x => x.StockLocationId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(256); RuleFor(x => x.AddressLine).MaximumLength(1000); } }
public sealed class UpdateStockLocationCommandHandler(IUnitOfWork uow, ICurrentUser currentUser) : IRequestHandler<UpdateStockLocationCommand, TResult<StockLocationDto>>
{
    public async Task<TResult<StockLocationDto>> Handle(UpdateStockLocationCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<StockLocationDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Inventory.LocationsManage)) return TResult<StockLocationDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var location = await uow.Repository<StockLocation>().FindByIdAsync(request.StockLocationId);
        if (location is null) return TResult<StockLocationDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (location.ConcurrencyStamp != request.ConcurrencyStamp) return TResult<StockLocationDto>.Failure(MessageKey.DataHasBeenChanged, ErrorCodes.ALREADY_EXISTS);
        location.UpdateDetails(request.Name, request.AdministrativeAreaId, request.AddressLine); location.SetActive(request.IsActive); location.ConcurrencyStamp = Guid.NewGuid();
        await uow.Repository<StockLocation>().UpdateAsync(location, ct);
        return TResult<StockLocationDto>.Success(new(location.Id, location.Code, location.Name, location.AdministrativeAreaId, location.AddressLine, location.IsActive, location.ConcurrencyStamp));
    }
}
