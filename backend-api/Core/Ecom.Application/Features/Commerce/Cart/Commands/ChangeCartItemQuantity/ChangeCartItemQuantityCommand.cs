using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart.Commands.ChangeCartItemQuantity;

public sealed record ChangeCartItemQuantityCommand(Guid CartItemId, int Quantity) : IRequest<TResult<CartDto>>, ITransactionalRequest;
public sealed class ChangeCartItemQuantityCommandValidator : AbstractValidator<ChangeCartItemQuantityCommand>
{ public ChangeCartItemQuantityCommandValidator() { RuleFor(x => x.CartItemId).NotEmpty(); RuleFor(x => x.Quantity).InclusiveBetween(1, 999); } }

public sealed class ChangeCartItemQuantityCommandHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver)
    : IRequestHandler<ChangeCartItemQuantityCommand, TResult<CartDto>>
{
    public async Task<TResult<CartDto>> Handle(ChangeCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<CartDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var now = DateTime.UtcNow;
        var cart = await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().Query().FirstOrDefaultAsync(principal.UserId.HasValue ? x => x.UserId == principal.UserId && x.Status == CartStatus.Active : x => x.GuestTokenHash == principal.GuestTokenHash && x.Status == CartStatus.Active, cancellationToken);
        if (cart is null) return TResult<CartDto>.Failure("Active cart was not found.", ErrorCodes.NOT_FOUND);
        if (cart.IsExpiredAt(now))
            return TResult<CartDto>.Failure("The active cart has expired.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var item = await unitOfWork.Repository<CartItem>().Query().FirstOrDefaultAsync(x => x.Id == request.CartItemId && x.CartId == cart.Id, cancellationToken);
        if (item is null) return TResult<CartDto>.Failure("Cart item was not found.", ErrorCodes.NOT_FOUND);
        cart.ChangeQuantity(item, request.Quantity);
        await unitOfWork.Repository<CartItem>().UpdateAsync(item, cancellationToken);
        var items = await unitOfWork.Repository<CartItem>().Query().Where(x => x.CartId == cart.Id).ToListAsync(cancellationToken);
        return TResult<CartDto>.Success(CartDtoMapper.Map(cart, items));
    }
}
