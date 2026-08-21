using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart.Commands.MergeGuestCart;

public sealed record MergeGuestCartCommand : IRequest<TResult<CartDto>>, ITransactionalRequest;
public sealed class MergeGuestCartCommandHandler(IUnitOfWork uow, ICurrentUser current, ICartPrincipalResolver principals,
    ICartMutationLock cartMutationLock)
    : IRequestHandler<MergeGuestCartCommand, TResult<CartDto>>
{
    public async Task<TResult<CartDto>> Handle(MergeGuestCartCommand request, CancellationToken ct)
    {
        if (!current.IsAuthenticated || current.UserId == Guid.Empty) return TResult<CartDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var guest = principals.ResolveGuestPrincipal(); if (guest is null) return TResult<CartDto>.Failure("Guest cart was not found.", ErrorCodes.NOT_FOUND);
        await cartMutationLock.AcquireForMergeAsync(current.UserId, guest.GuestTokenHash!, ct);
        var source = await uow.Repository<Ecom.Domain.Entities.Cart>().Query().FirstOrDefaultAsync(x => x.GuestTokenHash == guest.GuestTokenHash, ct);
        if (source is null) return TResult<CartDto>.Failure("Guest cart was not found.", ErrorCodes.NOT_FOUND);
        if (source.Status == CartStatus.Converted)
            return TResult<CartDto>.Success(await GetActiveUserCartAsync(ct));
        if (source.Status != CartStatus.Active)
            return TResult<CartDto>.Failure("Guest cart is no longer active.", ErrorCodes.UNPROCESSABLE_ENTITY);
        if (source.IsExpiredAt(DateTime.UtcNow))
            return TResult<CartDto>.Failure("Guest cart has expired.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var target = await uow.Repository<Ecom.Domain.Entities.Cart>().Query().FirstOrDefaultAsync(x => x.UserId == current.UserId && x.Status == CartStatus.Active, ct);
        if (target is null) { target = Ecom.Domain.Entities.Cart.CreateForUser(current.UserId); await uow.Repository<Ecom.Domain.Entities.Cart>().InsertAsync(target, ct); }
        var targetItems = await uow.Repository<CartItem>().Query().Where(x => x.CartId == target.Id).ToListAsync(ct);
        var sourceItems = await uow.Repository<CartItem>().Query().Where(x => x.CartId == source.Id).ToListAsync(ct);
        var before = targetItems.Select(x => x.Id).ToHashSet();
        target.MergeFrom(source, targetItems, sourceItems);
        foreach (var item in targetItems.Where(x => !before.Contains(x.Id))) await uow.Repository<CartItem>().InsertAsync(item, ct);
        foreach (var item in targetItems.Where(x => before.Contains(x.Id))) await uow.Repository<CartItem>().UpdateAsync(item, ct);
        await uow.Repository<Ecom.Domain.Entities.Cart>().UpdateAsync(source, ct);
        return TResult<CartDto>.Success(CartDtoMapper.Map(target, targetItems));
    }

    private async Task<CartDto> GetActiveUserCartAsync(CancellationToken ct)
    {
        var target = await uow.Repository<Ecom.Domain.Entities.Cart>().Query().FirstOrDefaultAsync(
            x => x.UserId == current.UserId && x.Status == CartStatus.Active, ct);
        if (target is null)
            return CartDto.Empty;

        var items = await uow.Repository<CartItem>().Query().Where(x => x.CartId == target.Id).ToListAsync(ct);
        return CartDtoMapper.Map(target, items);
    }
}
