using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Payments.Queries.GetSePayReconciliation;

public sealed record SePayReconciliationItemDto(
    Guid NotificationId,
    Guid? PaymentGatewayAttemptId,
    string NotificationType,
    string? InvoiceNumber,
    string? ExternalTransactionId,
    string? ProviderOrderStatus,
    string? ProviderTransactionStatus,
    string? FailureReasonCode,
    DateTime ReceivedAt,
    DateTime? OccurredAt);

public sealed record GetSePayReconciliationQuery : IRequest<TResult<IReadOnlyList<SePayReconciliationItemDto>>>;

public sealed class GetSePayReconciliationQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSePayReconciliationQuery, TResult<IReadOnlyList<SePayReconciliationItemDto>>>
{
    public async Task<TResult<IReadOnlyList<SePayReconciliationItemDto>>> Handle(GetSePayReconciliationQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.HasPolicy(Permissions.Payments.Verify))
            return TResult<IReadOnlyList<SePayReconciliationItemDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var notifications = await unitOfWork.Repository<PaymentGatewayNotification>().QueryNoTracking()
            .Where(x => x.Provider == "sepay" && x.Disposition == PaymentGatewayNotificationDisposition.NeedsReconciliation)
            .OrderByDescending(x => x.ReceivedAt)
            .Take(100)
            .Select(x => new SePayReconciliationItemDto(x.Id, x.PaymentGatewayAttemptId, x.NotificationType,
                x.InvoiceNumber, x.ExternalTransactionId, x.ProviderOrderStatus, x.ProviderTransactionStatus,
                x.FailureReasonCode, x.ReceivedAt, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return TResult<IReadOnlyList<SePayReconciliationItemDto>>.Success(notifications);
    }
}
