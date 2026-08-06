using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid CartItemId) : IRequest<TResult>, ITransactionalRequest;
public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand> { public RemoveCartItemCommandValidator() => RuleFor(x => x.CartItemId).NotEmpty(); }

public sealed class RemoveCartItemCommandHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver)
    : IRequestHandler<RemoveCartItemCommand, TResult>
{
    public async Task<TResult> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var now = DateTime.UtcNow;
        var cart = await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().Query().FirstOrDefaultAsync(principal.UserId.HasValue ? x => x.UserId == principal.UserId && x.Status == CartStatus.Active : x => x.GuestTokenHash == principal.GuestTokenHash && x.Status == CartStatus.Active, cancellationToken);
        if (cart is null) return TResult.Failure("Active cart was not found.", ErrorCodes.NOT_FOUND);
        if (cart.IsExpiredAt(now))
            return TResult.Failure("The active cart has expired.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var item = await unitOfWork.Repository<CartItem>().Query().FirstOrDefaultAsync(x => x.Id == request.CartItemId && x.CartId == cart.Id, cancellationToken);
        if (item is null) return TResult.Failure("Cart item was not found.", ErrorCodes.NOT_FOUND);
        cart.RemoveItem(item);
        await unitOfWork.Repository<CartItem>().DeleteAsync(item, cancellationToken);
        return TResult.Success();
    }
}
