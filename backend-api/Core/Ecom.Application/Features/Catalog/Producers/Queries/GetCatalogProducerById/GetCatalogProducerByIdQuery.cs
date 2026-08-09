using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Producers.Queries.GetCatalogProducerById;

public sealed record GetCatalogProducerByIdQuery(Guid ProducerId) : IRequest<TResult<CatalogProducerPickerDto>>;

public sealed class GetCatalogProducerByIdQueryHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductAccessService access)
    : IRequestHandler<GetCatalogProducerByIdQuery, TResult<CatalogProducerPickerDto>>
{
    public async Task<TResult<CatalogProducerPickerDto>> Handle(
        GetCatalogProducerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Create);
        if (!authorization.IsSuccess)
            return TResult<CatalogProducerPickerDto>.Failure(authorization.Error!, authorization.ErrorCode);

        var producer = await unitOfWork.Repository<Producer>().QueryNoTracking()
            .Where(x => x.Id == request.ProducerId && x.PublicStatus == PublicStatus.Published && x.IsVerified)
            .Select(x => new CatalogProducerPickerDto(x.Id, x.Code, x.Name, x.PublicStatus, x.IsVerified))
            .SingleOrDefaultAsync(cancellationToken);
        return producer is null
            ? TResult<CatalogProducerPickerDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND)
            : TResult<CatalogProducerPickerDto>.Success(producer);
    }
}
