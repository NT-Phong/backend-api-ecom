using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Behaviours;

/// <summary>
/// Normalizes every supported catalog-product mutation under one AuditLog entity key. It runs
/// inside UnitOfWorkBehavior, so handled failures and exceptions leave no orphan audit record.
/// </summary>
public sealed class CatalogAuditBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, ICatalogAuditWriter auditWriter)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private static readonly IReadOnlyDictionary<string, string> Actions = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["UpdateProductDetailsCommand"] = "catalog.product.details-updated",
        ["ReplaceProductCategoriesCommand"] = "catalog.product.categories-replaced",
        ["AttachProductMediaCommand"] = "catalog.product.media-attached",
        ["UpdateProductMediaCommand"] = "catalog.product.media-updated",
        ["SetPrimaryProductMediaCommand"] = "catalog.product.media-primary-set",
        ["RemoveProductMediaCommand"] = "catalog.product.media-removed",
        ["CreateProductVariantCommand"] = "catalog.product.variant-created",
        ["UpdateProductVariantCommand"] = "catalog.product.variant-updated",
        ["ActivateProductVariantCommand"] = "catalog.product.variant-activated",
        ["PauseProductVariantCommand"] = "catalog.product.variant-paused",
        ["DiscontinueProductVariantCommand"] = "catalog.product.variant-discontinued",
        ["CreateVariantPriceCommand"] = "catalog.product.price-created",
        ["SubmitProductForReviewCommand"] = "catalog.product.submitted",
        ["PublishProductCommand"] = "catalog.product.published",
        ["PauseProductCommand"] = "catalog.product.paused",
        ["DiscontinueProductCommand"] = "catalog.product.discontinued",
        ["RestoreDiscontinuedProductCommand"] = "catalog.product.restored",
        ["SoftDeleteProductCommand"] = "catalog.product.deleted"
    };

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!Actions.TryGetValue(typeof(TRequest).Name, out var action))
            return await next();

        var productId = ReadGuid(request, "ProductId");
        var before = productId == Guid.Empty
            ? null
            : Snapshot(await unitOfWork.Repository<Product>().FindByIdAsync(productId));

        var response = await next();
        if (response is not IResult { IsSuccess: true })
            return response;

        if (productId == Guid.Empty)
            productId = ReadGuidFromResponse(response);
        if (productId == Guid.Empty)
            return response;

        var product = await unitOfWork.Repository<Product>().FindByIdAsync(productId);
        var changedFields = typeof(TRequest).GetProperties()
            .Where(x => x.Name is not "ProductId" and not "ConcurrencyStamp")
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToArray();
        var after = new
        {
            Product = Snapshot(product) ?? new { id = productId, status = "Deleted" },
            ChangedFields = changedFields,
            Metadata = SafeMetadata(request)
        };
        await auditWriter.WriteAsync(action, productId, before, after, cancellationToken);
        return response;
    }

    private static object? Snapshot(Product? product) => product is null ? null : new
    {
        product.Id,
        product.Slug,
        Status = product.Status.ToString(),
        product.ConcurrencyStamp,
        product.ProducerId,
        product.IsDeleted
    };

    private static object SafeMetadata(TRequest request)
    {
        var requestType = request.GetType();
        return new
        {
            VariantId = ReadGuid(request, "VariantId"),
            MediaAssetId = ReadGuid(request, "MediaAssetId"),
            CategoryIds = requestType.GetProperty("Categories")?.GetValue(request) is IEnumerable<object> categories
                ? categories.Select(x => x.GetType().GetProperty("CategoryId")?.GetValue(x)).ToArray()
                : null,
            PriceListId = requestType.GetProperty("PriceListId")?.GetValue(request) as Guid?
        };
    }

    private static Guid ReadGuid(object value, string propertyName) => value.GetType().GetProperty(propertyName)?.GetValue(value) is Guid id
        ? id
        : Guid.Empty;

    private static Guid ReadGuidFromResponse(TResponse response)
    {
        var data = response?.GetType().GetProperty("Data")?.GetValue(response);
        if (data is null) return Guid.Empty;
        return ReadGuid(data, "ProductId") is var productId && productId != Guid.Empty
            ? productId
            : ReadGuid(data, "Id");
    }
}
