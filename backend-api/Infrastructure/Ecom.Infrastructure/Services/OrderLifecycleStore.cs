using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database;

namespace Ecom.Infrastructure.Services;

/// <summary>
/// Serializes state-changing order operations inside the UnitOfWork transaction.
/// Inventory locks still protect stock; these locks protect the order/payment/shipment state machine.
/// </summary>
public sealed class OrderLifecycleStore(ApplicationDbContext db) : IOrderLifecycleStore
{
    public Task<Order?> LockOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Orders.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_Order""
WHERE ""Id"" = {orderId} AND ""IsDeleted"" = false
FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Payment?> LockPaymentAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Payments.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_Payment""
WHERE ""OrderId"" = {orderId} AND ""IsDeleted"" = false
FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Shipment?> LockShipmentAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Shipments.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_Shipment""
WHERE ""OrderId"" = {orderId} AND ""IsDeleted"" = false
FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<PaymentGatewayAttempt?> LockPaymentGatewayAttemptAsync(string provider, string invoiceNumber,
        CancellationToken cancellationToken) =>
        db.PaymentGatewayAttempts.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_PaymentGatewayAttempt""
WHERE ""Provider"" = {provider} AND ""InvoiceNumber"" = {invoiceNumber} AND ""IsDeleted"" = false
FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<PaymentBankQrAttempt?> LockPaymentBankQrAttemptAsync(string provider, string paymentCode,
        CancellationToken cancellationToken) =>
        db.PaymentBankQrAttempts.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_PaymentBankQrAttempt""
WHERE ""Provider"" = {provider} AND ""PaymentCode"" = {paymentCode} AND ""IsDeleted"" = false
FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
}
