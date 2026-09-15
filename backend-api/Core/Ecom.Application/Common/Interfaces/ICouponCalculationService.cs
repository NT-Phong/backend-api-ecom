using Ecom.Application.Common.Commerce;
using Ecom.Domain.Constants;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Interfaces;

public sealed record CouponCalculationResult(
    bool IsValid,
    Coupon? Coupon,
    Promotion? Promotion,
    decimal DiscountAmount,
    string? ErrorMessage,
    ErrorCodes? ErrorCode = null)
{
    public static CouponCalculationResult Success(Coupon coupon, Promotion promotion, decimal discountAmount) =>
        new(true, coupon, promotion, discountAmount, null, null);

    public static CouponCalculationResult Fail(string message, ErrorCodes errorCode = ErrorCodes.UNPROCESSABLE_ENTITY) =>
        new(false, null, null, 0, message, errorCode);
}

public interface ICouponCalculationService
{
    Task<CouponCalculationResult> ValidateAndCalculateAsync(
        string couponCode,
        CartPrincipal principal,
        IReadOnlyList<CheckoutLine> lines,
        decimal subtotal,
        decimal shippingFee,
        CancellationToken cancellationToken,
        CouponValidationMode validationMode = CouponValidationMode.Quote);
}
