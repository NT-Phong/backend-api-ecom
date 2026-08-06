using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Payments.Commands.VerifyBankTransfer;
public sealed record VerifyBankTransferCommand(Guid OrderId, string ProviderReference) : IRequest<TResult>, ITransactionalRequest;
public sealed class VerifyBankTransferCommandValidator : AbstractValidator<VerifyBankTransferCommand> { public VerifyBankTransferCommandValidator() { RuleFor(x => x.OrderId).NotEmpty(); RuleFor(x => x.ProviderReference).NotEmpty().MaximumLength(200); } }
public sealed class VerifyBankTransferCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<VerifyBankTransferCommand, TResult>
{
    public async Task<TResult> Handle(VerifyBankTransferCommand r, CancellationToken ct)
    {
        if (!current.HasPolicy(Permissions.Payments.Verify)) return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var payment = await uow.Repository<Payment>().Query().FirstOrDefaultAsync(x => x.OrderId == r.OrderId && x.Method == PaymentMethod.BankTransfer, ct);
        if (payment is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var transaction = payment.MarkPaid(payment.Amount, "manual-bank-transfer", r.ProviderReference, DateTime.UtcNow);
        await uow.Repository<Payment>().UpdateAsync(payment, ct); await uow.Repository<PaymentTransaction>().InsertAsync(transaction, ct);
        return TResult.Success();
    }
}
