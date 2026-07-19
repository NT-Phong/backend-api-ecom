using Ecom.Domain.Entities;
using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Commands.UpdateProductDetails;

public sealed record UpdateProductDetailsCommand(Guid ProductId, Guid ConcurrencyStamp, string Name, string Slug,
    string? ShortDescription, string? Description, string? UsageInstructions, string? StorageInstructions,
    string? WarningText, string? MetaTitle, string? MetaDescription) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class UpdateProductDetailsCommandValidator : AbstractValidator<UpdateProductDetailsCommand>
{
    public UpdateProductDetailsCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300); RuleFor(x => x.Slug).NotEmpty().MaximumLength(350);
        RuleFor(x => x.ShortDescription).MaximumLength(1000); RuleFor(x => x.MetaTitle).MaximumLength(255); RuleFor(x => x.MetaDescription).MaximumLength(500);
    }
}

public sealed class UpdateProductDetailsCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<UpdateProductDetailsCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(UpdateProductDetailsCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId);
        if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp);
        if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        if (product.Slug != request.Slug.Trim() && await unitOfWork.Repository<Product>().AnyAsync([x => x.Slug == request.Slug.Trim()]))
            return TResult<ProductManagementResult>.Failure("Product slug already exists.", ErrorCodes.ALREADY_EXISTS);

        product.UpdateDetails(request.Name, request.Slug, request.ShortDescription, request.Description,
            request.UsageInstructions, request.StorageInstructions, request.WarningText, request.MetaTitle, request.MetaDescription);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
