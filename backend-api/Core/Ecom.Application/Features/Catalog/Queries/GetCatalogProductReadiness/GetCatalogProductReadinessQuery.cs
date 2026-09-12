using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Queries.GetCatalogProductReadiness;

public sealed record GetCatalogProductReadinessQuery(Guid ProductId)
    : IRequest<TResult<CatalogProductReadinessDto>>;

public sealed class GetCatalogProductReadinessQueryValidator : AbstractValidator<GetCatalogProductReadinessQuery>
{
    public GetCatalogProductReadinessQueryValidator() => RuleFor(x => x.ProductId).NotEmpty();
}

public sealed class GetCatalogProductReadinessQueryHandler(ICatalogProductAccessService access,
    ICatalogReadinessService readinessService)
    : IRequestHandler<GetCatalogProductReadinessQuery, TResult<CatalogProductReadinessDto>>
{
    public async Task<TResult<CatalogProductReadinessDto>> Handle(GetCatalogProductReadinessQuery request,
        CancellationToken cancellationToken)
    {
        var catalogAuthorization = access.Ensure(Permissions.CatalogProducts.Read);
        if (!catalogAuthorization.IsSuccess)
            return CatalogCommandSupport.Failure<CatalogProductReadinessDto>(catalogAuthorization);
        var inventoryAuthorization = access.Ensure(Permissions.Inventory.Read);
        if (!inventoryAuthorization.IsSuccess)
            return CatalogCommandSupport.Failure<CatalogProductReadinessDto>(inventoryAuthorization);
        var readiness = await readinessService.GetAsync(request.ProductId, cancellationToken);
        return readiness is null
            ? TResult<CatalogProductReadinessDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND)
            : TResult<CatalogProductReadinessDto>.Success(readiness);
    }
}
