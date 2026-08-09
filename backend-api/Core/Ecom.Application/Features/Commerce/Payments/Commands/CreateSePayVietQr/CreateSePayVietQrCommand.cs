using System.Security.Cryptography;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Payments.Commands.CreateSePayVietQr;

public sealed record SePayVietQrDto(Guid OrderId, string QrImageUrl, string BankCode, string VirtualAccountDisplay,
    string AccountHolder, decimal Amount, string CurrencyCode, string PaymentCode, DateTime ExpiresAt);
public sealed record CreateSePayVietQrCommand(Guid OrderId) : IRequest<TResult<SePayVietQrDto>>, ITransactionalRequest;
public sealed class CreateSePayVietQrCommandValidator : AbstractValidator<CreateSePayVietQrCommand>
{ public CreateSePayVietQrCommandValidator() => RuleFor(x => x.OrderId).NotEmpty(); }

public sealed class CreateSePayVietQrCommandHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver,
    IOrderLifecycleStore lifecycle, ISePayBankQrService service) : IRequestHandler<CreateSePayVietQrCommand, TResult<SePayVietQrDto>>
{
    private const string Provider = "sepay-bank-qr";
    public async Task<TResult<SePayVietQrDto>> Handle(CreateSePayVietQrCommand request, CancellationToken ct)
    {
        if (!service.IsEnabled) return TResult<SePayVietQrDto>.Failure("SePay VietQR is not available.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<SePayVietQrDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var order = await lifecycle.LockOrderAsync(request.OrderId, ct);
        if (order is null || (principal.UserId.HasValue ? order.UserId != principal.UserId : order.UserId is not null || order.GuestTokenHashSnapshot != principal.GuestTokenHash))
            return TResult<SePayVietQrDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var payment = await lifecycle.LockPaymentAsync(order.Id, ct);
        if (payment is null || payment.Method != PaymentMethod.SePayVietQr || payment.Status != PaymentStatus.Pending || payment.DueAt is null || payment.DueAt <= DateTime.UtcNow)
            return TResult<SePayVietQrDto>.Failure("This payment can no longer open VietQR.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var attempt = await unitOfWork.Repository<PaymentBankQrAttempt>().Query().SingleOrDefaultAsync(x => x.PaymentId == payment.Id && x.Provider == Provider, ct);
        if (attempt is null)
        {
            var code = service.PaymentCodePrefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
            attempt = PaymentBankQrAttempt.Create(payment.Id, Provider, code, payment.Amount, order.CurrencyCode, service.VirtualAccountFingerprint, payment.DueAt.Value);
            await unitOfWork.Repository<PaymentBankQrAttempt>().InsertAsync(attempt, ct);
        }
        if (attempt.Status is PaymentBankQrAttemptStatus.Paid or PaymentBankQrAttemptStatus.NeedsReconciliation)
            return TResult<SePayVietQrDto>.Failure("This QR payment attempt is no longer available.", ErrorCodes.UNPROCESSABLE_ENTITY);
        attempt.MarkQrIssued(DateTime.UtcNow); await unitOfWork.Repository<PaymentBankQrAttempt>().UpdateAsync(attempt, ct);
        var qr = service.CreateQrForm(attempt.ExpectedAmount, attempt.PaymentCode, attempt.ExpiresAt);
        return TResult<SePayVietQrDto>.Success(new(order.Id, qr.QrImageUrl, qr.BankCode, qr.VirtualAccountDisplay, qr.AccountHolder, qr.Amount, qr.CurrencyCode, qr.PaymentCode, qr.ExpiresAt));
    }
}
