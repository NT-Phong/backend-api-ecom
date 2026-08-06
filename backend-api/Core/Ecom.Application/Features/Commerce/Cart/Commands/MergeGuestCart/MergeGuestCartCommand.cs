using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart.Commands.MergeGuestCart;

public sealed record MergeGuestCartCommand : IRequest<TResult<CartDto>>, ITransactionalRequest;
public sealed class MergeGuestCartCommandHandler(IUnitOfWork uow, ICurrentUser current, ICartPrincipalResolver principals)
    : IRequestHandler<MergeGuestCartCommand, TResult<CartDto>>
{
    public async Task<TResult<CartDto>> Handle(MergeGuestCartCommand request, CancellationToken ct)
    {
        if (!current.IsAuthenticated || current.UserId == Guid.Empty) return TResult<CartDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var guest = principals.ResolveGuestPrincipal(); if (guest is null) return TResult<CartDto>.Failure("Guest cart was not found.", ErrorCodes.NOT_FOUND);
        var source = await uow.Repository<Ecom.Domain.Entities.Cart>().Query().FirstOrDefaultAsync(x => x.GuestTokenHash == guest.GuestTokenHash && x.Status == CartStatus.Active, ct);
        if (source is null) return TResult<CartDto>.Failure("Guest cart was not found.", ErrorCodes.NOT_FOUND);
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
        await uow.Repository<Ecom.Domain.Entities.Cart>().UpdateAsync(target, ct);
        principals.ClearGuestPrincipal();
        return TResult<CartDto>.Success(CartDtoMapper.Map(target, targetItems));
    }
}
