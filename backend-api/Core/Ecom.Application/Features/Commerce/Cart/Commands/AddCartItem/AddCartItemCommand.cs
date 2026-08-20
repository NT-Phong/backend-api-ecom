using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart.Commands.AddCartItem;

public sealed record AddCartItemCommand(Guid ProductVariantId, int Quantity) : IRequest<TResult<CartDto>>, ITransactionalRequest;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator() { RuleFor(x => x.ProductVariantId).NotEmpty(); RuleFor(x => x.Quantity).InclusiveBetween(1, 999); }
}

public sealed class AddCartItemCommandHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver,
    IEffectivePriceResolver effectivePriceResolver)
    : IRequestHandler<AddCartItemCommand, TResult<CartDto>>
{
    public async Task<TResult<CartDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var principal = principalResolver.ResolveOrCreateGuestPrincipal();
        var variant = await unitOfWork.Repository<ProductVariant>().QueryNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProductVariantId && x.Status == VariantStatus.Active, cancellationToken);
        if (variant is null || !await unitOfWork.Repository<Product>().AnyAsync([x => x.Id == variant.ProductId && x.Status == ProductStatus.Published]))
            return TResult<CartDto>.Failure("Product variant is unavailable.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var effectivePrices = await effectivePriceResolver.ResolveForVariantsAsync([variant.Id], now, cancellationToken);
        if (!effectivePrices.ContainsKey(variant.Id))
            return TResult<CartDto>.Failure("Product variant does not have an active price.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var cart = await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().Query().FirstOrDefaultAsync(
            principal.UserId.HasValue ? x => x.UserId == principal.UserId && x.Status == CartStatus.Active : x => x.GuestTokenHash == principal.GuestTokenHash && x.Status == CartStatus.Active, cancellationToken);
        if (cart is not null && cart.IsExpiredAt(now))
        {
            if (!principal.IsGuest)
                return TResult<CartDto>.Failure("The active cart has expired.", ErrorCodes.UNPROCESSABLE_ENTITY);

            cart.Expire();
            await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().UpdateAsync(cart, cancellationToken);
            principal = principalResolver.RotateGuestPrincipal();
            cart = null;
        }
        if (cart is null)
        {
            cart = principal.UserId.HasValue ? Ecom.Domain.Entities.Cart.CreateForUser(principal.UserId.Value) : Ecom.Domain.Entities.Cart.CreateForGuest(principal.GuestTokenHash!, now.AddDays(30));
            await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().InsertAsync(cart, cancellationToken);
        }
        var items = await unitOfWork.Repository<CartItem>().Query().Where(x => x.CartId == cart.Id).ToListAsync(cancellationToken);
        var existing = items.SingleOrDefault(x => x.ProductVariantId == request.ProductVariantId);
        var item = cart.AddItem(items, request.ProductVariantId, request.Quantity);
        if (existing is null) await unitOfWork.Repository<CartItem>().InsertAsync(item, cancellationToken);
        else await unitOfWork.Repository<CartItem>().UpdateAsync(item, cancellationToken);
        return TResult<CartDto>.Success(CartDtoMapper.Map(cart, items));
    }
}
