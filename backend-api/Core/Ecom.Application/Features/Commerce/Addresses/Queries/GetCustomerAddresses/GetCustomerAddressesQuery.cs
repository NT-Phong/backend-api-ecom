using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Addresses.Queries.GetCustomerAddresses;
public sealed record GetCustomerAddressesQuery : IRequest<TResult<IReadOnlyList<CustomerAddressDto>>>;
public sealed class GetCustomerAddressesQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser) : IRequestHandler<GetCustomerAddressesQuery, TResult<IReadOnlyList<CustomerAddressDto>>>
{
    public async Task<TResult<IReadOnlyList<CustomerAddressDto>>> Handle(GetCustomerAddressesQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty) return TResult<IReadOnlyList<CustomerAddressDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var values = await unitOfWork.Repository<CustomerAddress>().QueryNoTracking().Where(x => x.UserId == currentUser.UserId).OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.CreatedAt).ToListAsync(ct);
        return TResult<IReadOnlyList<CustomerAddressDto>>.Success(values.Select(CustomerAddressMapper.Map).ToList());
    }
}
