using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions;

public sealed record ValidateCouponDto(
    bool IsValid,
    string? Message,
    decimal DiscountAmount,
    decimal EstimatedTotal,
    Guid? CouponId,
    string? CouponCode,
    PromotionType? PromotionType,
    decimal? PromotionValue,
    decimal ShippingAmount = 0);

public sealed record AvailableCouponDto(
    Guid Id,
    string Code,
    string? PromotionName,
    PromotionType? PromotionType,
    decimal? PromotionValue,
    decimal? MinOrderAmount,
    DateTime StartsAt,
    DateTime? EndsAt);

public sealed record ManagementPromotionDto(
    Guid Id,
    string Code,
    string Name,
    PromotionType PromotionType,
    decimal Value,
    decimal? MinOrderAmount,
    PromotionStatus Status,
    DateTime StartsAt,
    DateTime? EndsAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ManagementCouponDto(
    Guid Id,
    Guid? PromotionId,
    string? PromotionName,
    string Code,
    int? UsageLimit,
    int UsageCount,
    int? PerUserLimit,
    CouponStatus Status,
    DateTime StartsAt,
    DateTime? EndsAt,
    IReadOnlyList<Guid> AppliedProductIds,
    IReadOnlyList<Guid> AppliedCategoryIds,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CouponRedemptionDto(
    Guid Id,
    Guid CouponId,
    Guid? UserId,
    Guid OrderId,
    decimal DiscountAmount,
    DateTime RedeemedAt);
