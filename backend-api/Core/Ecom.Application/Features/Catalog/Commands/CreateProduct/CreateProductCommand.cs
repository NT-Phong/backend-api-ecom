using Ecom.Domain.Entities;
using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Commands.CreateProduct;

public sealed record CreateProductCommand(Guid ProducerId, string Name, string Slug, string? ShortDescription,
    string? Description, string? UsageInstructions, string? StorageInstructions, string? WarningText,
    string? MetaTitle, string? MetaDescription) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(350);
        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.MetaTitle).MaximumLength(255);
        RuleFor(x => x.MetaDescription).MaximumLength(500);
    }
}

public sealed class CreateProductCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<CreateProductCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Create);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        if (!await unitOfWork.Repository<Producer>().ExistsAsync(request.ProducerId))
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (await unitOfWork.Repository<Product>().AnyAsync([x => x.Slug == request.Slug.Trim()]))
            return TResult<ProductManagementResult>.Failure("Product slug already exists.", ErrorCodes.ALREADY_EXISTS);

        var product = Product.Create(request.ProducerId, request.Name, request.Slug);
        product.UpdateDetails(request.Name, request.Slug, request.ShortDescription, request.Description,
            request.UsageInstructions, request.StorageInstructions, request.WarningText, request.MetaTitle, request.MetaDescription);
        await unitOfWork.Repository<Product>().InsertAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(new(product.Id, product.Slug, product.Status, product.ConcurrencyStamp));
    }
}
