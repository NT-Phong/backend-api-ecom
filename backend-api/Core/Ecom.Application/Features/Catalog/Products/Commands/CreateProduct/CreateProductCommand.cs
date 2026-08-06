using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(Guid ProducerId, string Name, string Slug, string? ShortDescription,
    string? Description, string? UsageInstructions, string? StorageInstructions, string? WarningText,
    string? MetaTitle, string? MetaDescription) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
