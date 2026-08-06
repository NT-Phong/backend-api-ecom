using Ecom.Domain.Entities;
using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Options;

public sealed record GetProductOptionsQuery(Guid ProductId) : IRequest<TResult<IReadOnlyList<ProductOptionDto>>>;

public sealed class GetProductOptionsQueryValidator : AbstractValidator<GetProductOptionsQuery>
{
    public GetProductOptionsQueryValidator() => RuleFor(x => x.ProductId).NotEmpty();
}

public sealed class GetProductOptionsQueryHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<GetProductOptionsQuery, TResult<IReadOnlyList<ProductOptionDto>>>
{
    public async Task<TResult<IReadOnlyList<ProductOptionDto>>> Handle(GetProductOptionsQuery request, CancellationToken ct)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Read);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<IReadOnlyList<ProductOptionDto>>(authorization);
        var exists = await unitOfWork.Repository<Product>().QueryNoTracking().AnyAsync(x => x.Id == request.ProductId, ct);
        if (!exists) return TResult<IReadOnlyList<ProductOptionDto>>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var options = await unitOfWork.Repository<ProductOption>().QueryNoTracking()
            .Where(x => x.ProductId == request.ProductId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code).ToListAsync(ct);
        var optionIds = options.Select(x => x.Id).ToList();
        var values = await unitOfWork.Repository<ProductOptionValue>().QueryNoTracking()
            .Where(x => optionIds.Contains(x.ProductOptionId)).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Value).ToListAsync(ct);
        IReadOnlyList<ProductOptionDto> result = options.Select(option => new ProductOptionDto(option.Id, option.Code, option.Name, option.DisplayOrder,
            values.Where(value => value.ProductOptionId == option.Id).Select(value => new ProductOptionValueDto(value.Id, value.Value, value.DisplayOrder)).ToList())).ToList();
        return TResult<IReadOnlyList<ProductOptionDto>>.Success(result);
    }
}
