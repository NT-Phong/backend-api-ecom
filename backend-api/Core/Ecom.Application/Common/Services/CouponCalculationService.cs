using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Application.Common.Services;

public sealed class CouponCalculationService(IUnitOfWork unitOfWork, ICouponRedemptionStore couponRedemptionStore)
    : ICouponCalculationService
{
    public async Task<CouponCalculationResult> ValidateAndCalculateAsync(
        string couponCode,
        CartPrincipal principal,
        IReadOnlyList<CheckoutLine> lines,
        decimal subtotal,
        decimal shippingFee,
        CancellationToken cancellationToken,
        CouponValidationMode validationMode = CouponValidationMode.Quote)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return CouponCalculationResult.Fail("Mã giảm giá không được để trống.", ErrorCodes.BAD_REQUEST);

        var normalizedCode = couponCode.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;

        Coupon? coupon;
        Promotion? promotion;
        if (validationMode == CouponValidationMode.Redemption)
        {
            var locked = await couponRedemptionStore.LockCouponAndPromotionAsync(normalizedCode, cancellationToken);
            coupon = locked?.Coupon;
            promotion = locked?.Promotion;
        }
        else
        {
            coupon = await unitOfWork.Repository<Coupon>().QueryNoTracking()
                .FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
            promotion = coupon?.PromotionId is { } promotionId
                ? await unitOfWork.Repository<Promotion>().QueryNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == promotionId, cancellationToken)
                : null;
        }

        if (coupon is null)
            return CouponCalculationResult.Fail("Mã giảm giá không tồn tại.", ErrorCodes.NOT_FOUND);

        if (!coupon.IsEligible(now))
            return CouponCalculationResult.Fail("Mã giảm giá đã hết hạn hoặc chưa được kích hoạt.", ErrorCodes.UNPROCESSABLE_ENTITY);

        if (!coupon.PromotionId.HasValue)
            return CouponCalculationResult.Fail("Mã giảm giá không liên kết với chương trình khuyến mãi nào.", ErrorCodes.UNPROCESSABLE_ENTITY);

        if (promotion is null || !promotion.IsActiveAt(now))
            return CouponCalculationResult.Fail("Chương trình khuyến mãi của mã này hiện không khả dụng.", ErrorCodes.UNPROCESSABLE_ENTITY);

        if (promotion.MinOrderAmount.HasValue && subtotal < promotion.MinOrderAmount.Value)
            return CouponCalculationResult.Fail($"Mã này chỉ áp dụng cho đơn hàng có giá trị từ {promotion.MinOrderAmount.Value:N0} đ.", ErrorCodes.UNPROCESSABLE_ENTITY);

        if (coupon.UsageLimit.HasValue)
        {
            var totalRedeemed = await unitOfWork.Repository<CouponRedemption>().QueryNoTracking()
                .CountAsync(x => x.CouponId == coupon.Id, cancellationToken);
            if (totalRedeemed >= coupon.UsageLimit.Value)
                return CouponCalculationResult.Fail("Mã giảm giá đã hết lượt sử dụng.", ErrorCodes.UNPROCESSABLE_ENTITY);
        }

        if (coupon.PerUserLimit.HasValue && principal.UserId.HasValue)
        {
            var userRedeemed = await unitOfWork.Repository<CouponRedemption>().QueryNoTracking()
                .CountAsync(x => x.CouponId == coupon.Id && x.UserId == principal.UserId.Value, cancellationToken);
            if (userRedeemed >= coupon.PerUserLimit.Value)
                return CouponCalculationResult.Fail("Bạn đã sử dụng hết số lượt cho phép của mã giảm giá này.", ErrorCodes.UNPROCESSABLE_ENTITY);
        }

        // Kiểm tra giới hạn sản phẩm cụ thể nếu có
        var restrictedProductIds = await unitOfWork.Repository<CouponProduct>().QueryNoTracking()
            .Where(x => x.CouponId == coupon.Id)
            .Select(x => x.ProductId)
            .ToListAsync(cancellationToken);

        if (restrictedProductIds.Count > 0)
        {
            var variantIds = lines.Select(x => x.ProductVariantId).Distinct().ToList();
            var productIdsInCart = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
                .Where(x => variantIds.Contains(x.Id))
                .Select(x => x.ProductId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var hasMatchingProduct = productIdsInCart.Any(pId => restrictedProductIds.Contains(pId));
            if (!hasMatchingProduct)
                return CouponCalculationResult.Fail("Mã giảm giá không áp dụng cho sản phẩm nào trong giỏ hàng.", ErrorCodes.UNPROCESSABLE_ENTITY);
        }

        // Kiểm tra giới hạn danh mục cụ thể nếu có
        var restrictedCategoryIds = await unitOfWork.Repository<CouponCategory>().QueryNoTracking()
            .Where(x => x.CouponId == coupon.Id)
            .Select(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        if (restrictedCategoryIds.Count > 0)
        {
            var variantIds = lines.Select(x => x.ProductVariantId).Distinct().ToList();
            var productIdsInCart = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
                .Where(x => variantIds.Contains(x.Id))
                .Select(x => x.ProductId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var categoriesInCart = await unitOfWork.Repository<ProductCategory>().QueryNoTracking()
                .Where(x => productIdsInCart.Contains(x.ProductId))
                .Select(x => x.CategoryId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var hasMatchingCategory = categoriesInCart.Any(cId => restrictedCategoryIds.Contains(cId));
            if (!hasMatchingCategory)
                return CouponCalculationResult.Fail("Mã giảm giá không áp dụng cho danh mục của các sản phẩm trong giỏ hàng.", ErrorCodes.UNPROCESSABLE_ENTITY);
        }

        var discountAmount = promotion.CalculateDiscount(subtotal, shippingFee);
        if (discountAmount <= 0 && promotion.PromotionType != PromotionType.FreeShipping)
            return CouponCalculationResult.Fail("Đơn hàng không đủ điều kiện nhận chiết khấu.", ErrorCodes.UNPROCESSABLE_ENTITY);

        return CouponCalculationResult.Success(coupon, promotion, discountAmount);
    }
}
