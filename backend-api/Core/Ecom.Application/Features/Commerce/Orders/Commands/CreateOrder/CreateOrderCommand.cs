using System.Security.Cryptography;
using System.Text;
using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;
using Ecom.Domain.Models.Commerce;

namespace Ecom.Application.Features.Commerce.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(IReadOnlyList<Guid> CartItemIds, string RecipientName, string RecipientPhone,
    string ShippingAddress, Guid? AdministrativeAreaId, string? CustomerEmail, PaymentMethod PaymentMethod,
    string QuoteFingerprint, string IdempotencyKey, string ShippingMethodCode = "standard") : IRequest<TResult<OrderSummaryDto>>, ITransactionalRequest;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CartItemIds).NotEmpty(); RuleForEach(x => x.CartItemIds).NotEmpty();
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200); RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(1000); RuleFor(x => x.PaymentMethod).IsInEnum().NotEqual(PaymentMethod.Gateway);
        RuleFor(x => x.QuoteFingerprint).Length(64); RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200); RuleFor(x => x.ShippingMethodCode).Equal("standard").WithMessage("Only standard shipping is supported.");
    }
}

public sealed class CreateOrderCommandHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver,
    ICheckoutPricingService pricing, IInventoryReservationStore inventoryStore, IIdempotencyStore idempotencyStore,
    IOrderNumberGenerator orderNumberGenerator)
    : IRequestHandler<CreateOrderCommand, TResult<OrderSummaryDto>>
{
    public async Task<TResult<OrderSummaryDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<OrderSummaryDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var recipient = new CheckoutRecipient(request.RecipientName, request.RecipientPhone, request.ShippingAddress,
            request.AdministrativeAreaId, request.CustomerEmail);
        var begin = await idempotencyStore.BeginAsync("orders.create", principal.OwnerScope, request.IdempotencyKey,
            CreateRequestFingerprint(request, recipient), DateTime.UtcNow.AddHours(24), cancellationToken);
        if (begin.Kind == IdempotencyBeginKind.Mismatch)
            return TResult<OrderSummaryDto>.Failure("Idempotency key was reused with a different request.", ErrorCodes.ALREADY_EXISTS);
        if (begin.Kind == IdempotencyBeginKind.Processing)
            return TResult<OrderSummaryDto>.Failure("An identical order request is already processing.", ErrorCodes.ALREADY_EXISTS);
        if (begin.Kind == IdempotencyBeginKind.Completed)
        {
            var previous = await unitOfWork.Repository<Order>().FindByIdAsync(begin.Record.OrderId!.Value);
            var previousPayment = await unitOfWork.Repository<Payment>().FindOneAsync([x => x.OrderId == previous!.Id]);
            return TResult<OrderSummaryDto>.Success(new(previous!.Id, previous.OrderNumber, previous.Status, previousPayment!.Status, previous.GrandTotalAmount, previous.PlacedAt));
        }

        var quoteResult = await pricing.CreateQuoteAsync(principal, request.CartItemIds, recipient, request.PaymentMethod, cancellationToken);
        if (!quoteResult.IsSuccess) return TResult<OrderSummaryDto>.Failure(quoteResult.Error!, quoteResult.ErrorCode);
        var quote = quoteResult.Data;
        if (!string.Equals(quote.Fingerprint, request.QuoteFingerprint, StringComparison.Ordinal))
            return TResult<OrderSummaryDto>.Failure("Checkout price or availability has changed.", ErrorCodes.ALREADY_EXISTS);

        var trackedRequests = quote.Lines.Where(x => x.IsTracked)
            .Select(x => new InventoryLockRequest(x.ProductVariantId, x.Quantity)).ToList();
        var lockResult = await inventoryStore.LockTrackedInventoryAsync(trackedRequests, cancellationToken);
        if (!lockResult.IsSuccess) return TResult<OrderSummaryDto>.Failure(lockResult.Error!, lockResult.ErrorCode);

        var now = DateTime.UtcNow;
        var orderItems = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var order = Order.Create(orderNumberGenerator.Create(now), principal.UserId, principal.GuestTokenHash, recipient.CustomerEmail,
            principal.UserId.HasValue ? request.RecipientPhone : request.RecipientPhone, request.RecipientName,
            request.RecipientPhone, request.AdministrativeAreaId, request.ShippingAddress, quote.ShippingAmount, now,
            quote.Lines.Select(x => new OrderLineSnapshot(x.ProductVariantId, x.ProductName, x.VariantName, x.Sku, x.UnitPrice, x.Quantity)), orderItems, history);
        await unitOfWork.Repository<Order>().InsertAsync(order, cancellationToken);
        await unitOfWork.Repository<OrderItem>().InsertRangeAsync(orderItems, cancellationToken);
        await unitOfWork.Repository<OrderStatusHistory>().InsertRangeAsync(history, cancellationToken);

        var payment = Payment.Create(order.Id, request.PaymentMethod, order.GrandTotalAmount, now.AddMinutes(30));
        await unitOfWork.Repository<Payment>().InsertAsync(payment, cancellationToken);

        foreach (var line in quote.Lines.Where(x => x.IsTracked))
        {
            var orderItem = orderItems.Single(x => x.ProductVariantId == line.ProductVariantId);
            var locked = lockResult.Data[line.ProductVariantId];
            var movement = locked.Level.Reserve(line.Quantity, now, orderItem.Id);
            var reservation = InventoryReservation.Create(locked.InventoryItemId, locked.Level.StockLocationId, orderItem.Id,
                line.Quantity, now.AddMinutes(30));
            await unitOfWork.Repository<InventoryMovement>().InsertAsync(movement, cancellationToken);
            await unitOfWork.Repository<InventoryReservation>().InsertAsync(reservation, cancellationToken);
            await unitOfWork.Repository<InventoryLevel>().UpdateAsync(locked.Level, cancellationToken);
        }

        var cart = await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().Query().FirstAsync(principal.UserId.HasValue ? x => x.UserId == principal.UserId && x.Status == CartStatus.Active : x => x.GuestTokenHash == principal.GuestTokenHash && x.Status == CartStatus.Active, cancellationToken);
        var cartItems = await unitOfWork.Repository<CartItem>().Query().Where(x => x.CartId == cart.Id).ToListAsync(cancellationToken);
        cart.CheckoutSelectedItems(cartItems, request.CartItemIds);
        foreach (var item in cartItems.Where(x => x.IsDeleted)) await unitOfWork.Repository<CartItem>().DeleteAsync(item, cancellationToken);
        await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().UpdateAsync(cart, cancellationToken);

        begin.Record.Complete(order.Id);
        await unitOfWork.Repository<IdempotencyRecord>().UpdateAsync(begin.Record, cancellationToken);
        return TResult<OrderSummaryDto>.Success(new(order.Id, order.OrderNumber, order.Status, payment.Status, order.GrandTotalAmount, order.PlacedAt));
    }

    private static string CreateRequestFingerprint(CreateOrderCommand request, CheckoutRecipient recipient)
    {
        var canonical = string.Join(',', request.CartItemIds.OrderBy(x => x)) + $"|{request.QuoteFingerprint}|{request.PaymentMethod}|{request.ShippingMethodCode}|{recipient.RecipientName.Trim()}|{recipient.RecipientPhone.Trim()}|{recipient.ShippingAddress.Trim()}|{recipient.AdministrativeAreaId:N}|{recipient.CustomerEmail?.Trim()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}
