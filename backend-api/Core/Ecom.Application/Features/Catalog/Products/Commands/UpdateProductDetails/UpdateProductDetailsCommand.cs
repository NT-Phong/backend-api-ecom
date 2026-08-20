using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Products.Commands.UpdateProductDetails;

public sealed record UpdateProductDetailsCommand(Guid ProductId, Guid ConcurrencyStamp, string Name, string Slug,
    string? ShortDescription, string? Description, string? UsageInstructions, string? StorageInstructions,
    string? WarningText, string? MetaTitle, string? MetaDescription, string? BrandName = null) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
