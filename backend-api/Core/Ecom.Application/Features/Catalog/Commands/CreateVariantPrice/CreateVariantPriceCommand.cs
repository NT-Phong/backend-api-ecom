using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.CreateVariantPrice;

public sealed record CreateVariantPriceCommand(Guid ProductId, Guid VariantId, Guid ConcurrencyStamp, decimal Amount, PriceType PriceType,
    DateTime EffectiveFrom, DateTime? EffectiveTo, Guid? PriceListId, string CurrencyCode, int MinQuantity)
    : IRequest<TResult<VariantPriceManagementResult>>, ITransactionalRequest;

public sealed class CreateVariantPriceCommandValidator : AbstractValidator<CreateVariantPriceCommand>
{
    public CreateVariantPriceCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.VariantId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3); RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.EffectiveFrom).NotEqual(default(DateTime));
        RuleFor(x => x).Must(x => x.EffectiveTo is null || x.EffectiveTo > x.EffectiveFrom).WithMessage("Price effective window is invalid.");
    }
}

public sealed class CreateVariantPriceCommandHandler(IUnitOfWork unitOfWork, ICatalogProductMutationService mutation)
    : IRequestHandler<CreateVariantPriceCommand, TResult<VariantPriceManagementResult>>
{
    public async Task<TResult<VariantPriceManagementResult>> Handle(CreateVariantPriceCommand request, CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<VariantPriceManagementResult>(loaded);
        var product = loaded.Data;
        product.EnsureContentCanBeChanged();
        var variant = await unitOfWork.Repository<ProductVariant>().FindByIdAsync(request.VariantId);
        if (variant is null || variant.ProductId != request.ProductId) return TResult<VariantPriceManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        variant.EnsurePricingCanBeChanged();
        if (request.PriceListId.HasValue && !await unitOfWork.Repository<PriceList>().ExistsAsync(request.PriceListId.Value))
            return TResult<VariantPriceManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var price = VariantPrice.Create(variant.Id, request.Amount, request.PriceType, request.EffectiveFrom,
            request.EffectiveTo, request.PriceListId, request.CurrencyCode, request.MinQuantity);
        await unitOfWork.Repository<VariantPrice>().InsertAsync(price, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<VariantPriceManagementResult>.Success(new(price.Id, product.Id, result.ConcurrencyStamp));
    }
}
