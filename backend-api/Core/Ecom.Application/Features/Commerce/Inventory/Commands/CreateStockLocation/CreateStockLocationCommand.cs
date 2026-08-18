using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Commands.CreateStockLocation;

public sealed record CreateStockLocationCommand(string Code, string Name, Guid? AdministrativeAreaId, string? AddressLine)
    : IRequest<TResult<StockLocationDto>>, ITransactionalRequest;
public sealed class CreateStockLocationCommandValidator : AbstractValidator<CreateStockLocationCommand>
{ public CreateStockLocationCommandValidator() { RuleFor(x => x.Code).NotEmpty().MaximumLength(64); RuleFor(x => x.Name).NotEmpty().MaximumLength(256); RuleFor(x => x.AddressLine).MaximumLength(1000); } }
public sealed class CreateStockLocationCommandHandler(IUnitOfWork uow, ICurrentUser currentUser) : IRequestHandler<CreateStockLocationCommand, TResult<StockLocationDto>>
{
    public async Task<TResult<StockLocationDto>> Handle(CreateStockLocationCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<StockLocationDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Inventory.LocationsManage)) return TResult<StockLocationDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        if (await uow.Repository<StockLocation>().AnyAsync([x => x.Code == request.Code.Trim()])) return TResult<StockLocationDto>.Failure("Stock location code already exists.", ErrorCodes.ALREADY_EXISTS);
        var location = StockLocation.Create(request.Code, request.Name, request.AdministrativeAreaId, request.AddressLine);
        await uow.Repository<StockLocation>().InsertAsync(location, ct);
        return TResult<StockLocationDto>.Success(new(location.Id, location.Code, location.Name, location.AdministrativeAreaId, location.AddressLine, location.IsActive, location.ConcurrencyStamp));
    }
}
