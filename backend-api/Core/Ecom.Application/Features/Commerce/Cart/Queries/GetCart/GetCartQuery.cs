using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart.Queries.GetCart;

public sealed record GetCartQuery : IRequest<TResult<CartDto>>;

public sealed class GetCartQueryHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver)
    : IRequestHandler<GetCartQuery, TResult<CartDto>>
{
    public async Task<TResult<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<CartDto>.Success(CartDto.Empty);
        var cart = await unitOfWork.Repository<Ecom.Domain.Entities.Cart>().QueryNoTracking().FirstOrDefaultAsync(
            principal.UserId.HasValue
                ? x => x.UserId == principal.UserId && x.Status == CartStatus.Active
                : x => x.GuestTokenHash == principal.GuestTokenHash && x.Status == CartStatus.Active, cancellationToken);
        if (cart is null) return TResult<CartDto>.Success(CartDto.Empty);
        var items = await unitOfWork.Repository<CartItem>().QueryNoTracking().Where(x => x.CartId == cart.Id).ToListAsync(cancellationToken);
        return TResult<CartDto>.Success(CartDtoMapper.Map(cart, items));
    }
}
