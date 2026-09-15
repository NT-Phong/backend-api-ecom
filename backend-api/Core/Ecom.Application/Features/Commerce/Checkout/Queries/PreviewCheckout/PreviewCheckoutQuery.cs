using Ecom.Application.Common.Commerce;
using Ecom.Application.Features.Commerce.Checkout;

namespace Ecom.Application.Features.Commerce.Checkout.Queries.PreviewCheckout;

public sealed record PreviewCheckoutQuery(IReadOnlyList<Guid> CartItemIds, string RecipientName, string RecipientPhone,
    string ShippingAddress, Guid? AdministrativeAreaId, string? CustomerEmail, PaymentMethod PaymentMethod, string ShippingMethodCode = "standard",
    string? CouponCode = null, string? PackagingOption = null)
    : IRequest<TResult<CheckoutPreviewDto>>;

public sealed class PreviewCheckoutQueryValidator : AbstractValidator<PreviewCheckoutQuery>
{
    public PreviewCheckoutQueryValidator()
    {
        RuleFor(x => x.CartItemIds).NotEmpty(); RuleForEach(x => x.CartItemIds).NotEmpty();
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200); RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(1000); RuleFor(x => x.PaymentMethod).IsInEnum().NotEqual(PaymentMethod.Gateway); RuleFor(x => x.ShippingMethodCode).Equal("standard").WithMessage("Only standard shipping is supported.");
        RuleFor(x => x.CouponCode).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.CouponCode));
        RuleFor(x => x.PackagingOption).Must(CheckoutPackaging.IsSupported)
            .WithMessage("Packaging option must be standard or cold_chain.");
    }
}

public sealed class PreviewCheckoutQueryHandler(ICartPrincipalResolver principalResolver, ICheckoutPricingService pricing)
    : IRequestHandler<PreviewCheckoutQuery, TResult<CheckoutPreviewDto>>
{
    public async Task<TResult<CheckoutPreviewDto>> Handle(PreviewCheckoutQuery request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<CheckoutPreviewDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!CheckoutPackaging.TryNormalize(request.PackagingOption, out var packagingOption))
            return TResult<CheckoutPreviewDto>.Failure("Packaging option is not supported.", ErrorCodes.BAD_REQUEST);
        var quote = await pricing.CreateQuoteAsync(principal, request.CartItemIds,
            new CheckoutRecipient(request.RecipientName, request.RecipientPhone, request.ShippingAddress,
                request.AdministrativeAreaId, request.CustomerEmail), request.PaymentMethod, cancellationToken,
            request.CouponCode, packagingOption);
        return quote.IsSuccess ? TResult<CheckoutPreviewDto>.Success(CheckoutDtoMapper.Map(quote.Data))
            : TResult<CheckoutPreviewDto>.Failure(quote.Error!, quote.ErrorCode);
    }
}
