using Ecom.Domain.Entities;
using Ecom.Domain.Enums;

namespace Ecom.Application.Features.Commerce.System;

public sealed record CheckoutShippingSettingDto(decimal StandardFeeVnd, bool Exists, Guid? ConcurrencyStamp);
public sealed record AuditLogDto(Guid Id, Guid? ActorUserId, string Action, string EntityName, Guid? EntityId, Guid? CorrelationId,
    DateTime OccurredAt);
public sealed record ManagementUserSessionDto(Guid Id, Guid UserId, string? UserName, string? PhoneNumber, SessionClientType ClientType,
    AuthenticationMethod AuthenticationMethod, AuthenticationStrength AuthenticationStrength, string? DeviceId, DateTime CreatedAt,
    DateTime LastSeenAt, DateTime IdleExpiresAt, DateTime AbsoluteExpiresAt, DateTime? RevokedAt, string? RevocationReason);
public sealed record ManagementSecurityEventDto(Guid Id, Guid? UserId, Guid? SessionId, string EventType, SecurityRiskLevel RiskLevel,
    bool Success, DateTime OccurredAt);
