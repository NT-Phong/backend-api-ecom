using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Payments.Commands.CreateSePayCheckout;

public sealed record SePayCheckoutDto(Guid OrderId, string ActionUrl, string Method, IReadOnlyList<SePayCheckoutField> Fields);

public sealed record CreateSePayCheckoutCommand(Guid OrderId) : IRequest<TResult<SePayCheckoutDto>>, ITransactionalRequest;

public sealed class CreateSePayCheckoutCommandValidator : AbstractValidator<CreateSePayCheckoutCommand>
{
    public CreateSePayCheckoutCommandValidator() => RuleFor(x => x.OrderId).NotEmpty();
}

public sealed class CreateSePayCheckoutCommandHandler(
    IUnitOfWork unitOfWork,
    ICartPrincipalResolver principalResolver,
    IOrderLifecycleStore orderLifecycleStore,
    ISePayCheckoutService sePayCheckoutService)
    : IRequestHandler<CreateSePayCheckoutCommand, TResult<SePayCheckoutDto>>
{
    public async Task<TResult<SePayCheckoutDto>> Handle(CreateSePayCheckoutCommand request, CancellationToken cancellationToken)
    {
        if (!sePayCheckoutService.IsEnabled)
            return TResult<SePayCheckoutDto>.Failure("SePay checkout is not available.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null)
            return TResult<SePayCheckoutDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        var order = await orderLifecycleStore.LockOrderAsync(request.OrderId, cancellationToken);
        if (order is null || !OwnsOrder(order, principal))
            return TResult<SePayCheckoutDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var payment = await orderLifecycleStore.LockPaymentAsync(order.Id, cancellationToken);
        if (payment is null || payment.Method != PaymentMethod.SePay)
            return TResult<SePayCheckoutDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var now = DateTime.UtcNow;
        if (payment.Status != PaymentStatus.Pending || payment.DueAt is null || payment.DueAt <= now)
            return TResult<SePayCheckoutDto>.Failure("This payment can no longer open SePay checkout.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var attempts = unitOfWork.Repository<PaymentGatewayAttempt>();
        var attempt = await attempts.Query()
            .SingleOrDefaultAsync(x => x.PaymentId == payment.Id && x.Provider == "sepay", cancellationToken);
        if (attempt is null)
        {
            attempt = PaymentGatewayAttempt.Create(payment.Id, "sepay", $"SP-{order.OrderNumber}", payment.Amount,
                order.CurrencyCode, payment.DueAt.Value);
            await attempts.InsertAsync(attempt, cancellationToken);
        }
        else if (attempt.Status is PaymentGatewayAttemptStatus.Paid or PaymentGatewayAttemptStatus.NeedsReconciliation)
        {
            return TResult<SePayCheckoutDto>.Failure("This payment attempt is no longer available.", ErrorCodes.UNPROCESSABLE_ENTITY);
        }

        attempt.MarkCheckoutIssued(now);
        await attempts.UpdateAsync(attempt, cancellationToken);

        var form = sePayCheckoutService.CreateCheckoutForm(new SePayCheckoutRequest(order.Id, attempt.InvoiceNumber,
            payment.Amount, order.OrderNumber, order.UserId));
        return TResult<SePayCheckoutDto>.Success(new(order.Id, form.ActionUrl, form.Method, form.Fields));
    }

    private static bool OwnsOrder(Order order, CartPrincipal principal) => principal.UserId.HasValue
        ? order.UserId == principal.UserId
        : order.UserId is null && order.GuestTokenHashSnapshot == principal.GuestTokenHash;
}
