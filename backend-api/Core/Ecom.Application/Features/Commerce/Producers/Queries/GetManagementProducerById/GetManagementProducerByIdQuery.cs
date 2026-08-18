using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Producers.Queries.GetManagementProducerById;

public sealed record GetManagementProducerByIdQuery(Guid ProducerId) : IRequest<TResult<ProducerManagementDto>>;

public sealed class GetManagementProducerByIdQueryHandler(IUnitOfWork unitOfWork, ProducerManagementService service)
    : IRequestHandler<GetManagementProducerByIdQuery, TResult<ProducerManagementDto>>
{
    public async Task<TResult<ProducerManagementDto>> Handle(GetManagementProducerByIdQuery request, CancellationToken ct)
    {
        var authorization = service.Ensure(Permissions.Producers.Read);
        if (!authorization.IsSuccess) return TResult<ProducerManagementDto>.Failure(authorization.Error!, authorization.ErrorCode);
        var producer = await unitOfWork.Repository<Producer>().QueryNoTracking().SingleOrDefaultAsync(x => x.Id == request.ProducerId, ct);
        if (producer is null) return TResult<ProducerManagementDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var contacts = await unitOfWork.Repository<ProducerContact>().QueryNoTracking().Where(x => x.ProducerId == producer.Id)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync(ct);
        var facilities = await unitOfWork.Repository<ProductionFacility>().QueryNoTracking().Where(x => x.ProducerId == producer.Id)
            .OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(ct);
        return TResult<ProducerManagementDto>.Success(new ProducerManagementDto(producer.Id, producer.Code, producer.Name,
            producer.LegalName, producer.Description, producer.WebsiteUrl, producer.PublicStatus, producer.IsVerified,
            producer.VerifiedAt, producer.VerifiedByUserId, producer.ConcurrencyStamp, contacts.Select(ProducerManagementService.Map).ToList(),
            facilities.Select(ProducerManagementService.Map).ToList()));
    }
}
