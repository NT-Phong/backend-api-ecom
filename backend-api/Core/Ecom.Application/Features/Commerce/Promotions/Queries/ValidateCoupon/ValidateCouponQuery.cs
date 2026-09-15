using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Queries.ValidateCoupon;

public sealed record ValidateCouponQuery(
    string Code,
    IReadOnlyList<Guid>? CartItemIds = null,
    string? PackagingOption = null) : IRequest<TResult<ValidateCouponDto>>;

public sealed class ValidateCouponQueryValidator : AbstractValidator<ValidateCouponQuery>
{
    public ValidateCouponQueryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Mã giảm giá không được để trống.").MaximumLength(50);
        RuleFor(x => x.PackagingOption).Must(CheckoutPackaging.IsSupported)
            .WithMessage("Packaging option must be standard or cold_chain.");
    }
}

public sealed class ValidateCouponQueryHandler(
    IUnitOfWork unitOfWork,
    ICartPrincipalResolver principalResolver,
    IEffectivePriceResolver priceResolver,
    ICouponCalculationService couponCalculationService,
    ICheckoutShippingService shippingService)
    : IRequestHandler<ValidateCouponQuery, TResult<ValidateCouponDto>>
{
    public async Task<TResult<ValidateCouponDto>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null)
        {
            return TResult<ValidateCouponDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        }

        var now = DateTime.UtcNow;
        var lines = new List<CheckoutLine>();
        decimal subtotal = 0;

        // If user has an active cart, load lines to evaluate conditions (e.g. min spend, product/category scope)
        var cart = await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().FindOneAsync(
            principal.UserId.HasValue
                ? [x => x.UserId == principal.UserId && x.Status == CartStatus.Active && (x.ExpiresAt == null || x.ExpiresAt > now)]
                : [x => x.GuestTokenHash == principal.GuestTokenHash && x.Status == CartStatus.Active && (x.ExpiresAt == null || x.ExpiresAt > now)]);

        if (cart is not null)
        {
            var itemQuery = unitOfWork.Repository<CartItem>().QueryNoTracking().Where(x => x.CartId == cart.Id);
            if (request.CartItemIds is not null && request.CartItemIds.Count > 0)
            {
                itemQuery = itemQuery.Where(x => request.CartItemIds.Contains(x.Id));
            }

            var items = await itemQuery.ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                var variantIds = items.Select(x => x.ProductVariantId).Distinct().ToArray();
                var variants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
                    .Where(x => variantIds.Contains(x.Id) && x.Status == VariantStatus.Active)
                    .ToListAsync(cancellationToken);

                var productIds = variants.Select(x => x.ProductId).Distinct().ToArray();
                var products = await unitOfWork.Repository<Product>().QueryNoTracking()
                    .Where(x => productIds.Contains(x.Id) && x.Status == ProductStatus.Published)
                    .ToListAsync(cancellationToken);

                var prices = await priceResolver.ResolveForVariantsAsync(variantIds, now, cancellationToken);

                foreach (var item in items)
                {
                    var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                    if (variant is null) continue;

                    var product = products.FirstOrDefault(p => p.Id == variant.ProductId);
                    if (product is null) continue;

                    if (!prices.TryGetValue(variant.Id, out var resolvedPrice)) continue;
                    var unitPrice = resolvedPrice.Amount;

                    lines.Add(new CheckoutLine(
                        item.Id,
                        variant.Id,
                        product.Name,
                        variant.Name,
                        variant.Sku,
                        item.Quantity,
                        unitPrice,
                        variant.InventoryMode == InventoryMode.Tracked));
                }

                subtotal = lines.Sum(x => x.UnitPrice * x.Quantity);
            }
        }

        var shippingResult = await shippingService.ResolveShippingAmountAsync(request.PackagingOption, cancellationToken);
        if (!shippingResult.IsSuccess)
            return TResult<ValidateCouponDto>.Failure(shippingResult.Error!, shippingResult.ErrorCode);
        var shippingAmount = shippingResult.Data;

        var calcResult = await couponCalculationService.ValidateAndCalculateAsync(
            request.Code, principal, lines, subtotal, shippingAmount, cancellationToken);

        if (!calcResult.IsValid)
        {
            return TResult<ValidateCouponDto>.Success(new ValidateCouponDto(
                IsValid: false,
                Message: calcResult.ErrorMessage ?? "Mã giảm giá không hợp lệ.",
                DiscountAmount: 0,
                EstimatedTotal: subtotal + shippingAmount,
                CouponId: null,
                CouponCode: request.Code.Trim().ToUpperInvariant(),
                PromotionType: null,
                PromotionValue: null,
                ShippingAmount: shippingAmount));
        }

        var coupon = calcResult.Coupon!;
        var promotion = calcResult.Promotion!;
        return TResult<ValidateCouponDto>.Success(new ValidateCouponDto(
            IsValid: true,
            Message: "Áp dụng mã giảm giá thành công!",
            DiscountAmount: calcResult.DiscountAmount,
            EstimatedTotal: Math.Max(0, subtotal - calcResult.DiscountAmount + shippingAmount),
            CouponId: coupon.Id,
            CouponCode: coupon.Code,
            PromotionType: promotion.PromotionType,
            PromotionValue: promotion.Value,
            ShippingAmount: shippingAmount));
    }
}
