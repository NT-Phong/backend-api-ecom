using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Producers;

public sealed class ProducerManagementService(IUnitOfWork unitOfWork, ICurrentUser currentUser)
{
    public TResult Ensure(string permission) => !currentUser.IsAuthenticated
        ? TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED)
        : currentUser.HasPolicy(permission)
            ? TResult.Success()
            : TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

    public async Task<TResult<Producer>> LoadAsync(Guid producerId, Guid concurrencyStamp, string permission, CancellationToken ct)
    {
        var authorization = Ensure(permission);
        if (!authorization.IsSuccess) return TResult<Producer>.Failure(authorization.Error!, authorization.ErrorCode);
        var producer = await unitOfWork.Repository<Producer>().FindByIdAsync(producerId);
        if (producer is null) return TResult<Producer>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (producer.ConcurrencyStamp != concurrencyStamp)
            return TResult<Producer>.Failure(MessageKey.DataHasBeenChanged, ErrorCodes.ALREADY_EXISTS);
        return TResult<Producer>.Success(producer);
    }

    public async Task<TResult> EnsureCodeAvailableAsync(string code, Guid? currentId, CancellationToken ct)
    {
        var normalized = code.Trim();
        var exists = await unitOfWork.Repository<Producer>().AnyAsync([x => x.Code == normalized && (!currentId.HasValue || x.Id != currentId.Value)]);
        return exists
            ? TResult.Failure("Producer code already exists.", ErrorCodes.ALREADY_EXISTS)
            : TResult.Success();
    }

    public static ProducerContactDto Map(ProducerContact entity) => new(entity.Id, entity.ContactType, entity.ContactValue,
        entity.ContactName, entity.IsPublic, entity.DisplayOrder);

    public static ProductionFacilityDto Map(ProductionFacility entity) => new(entity.Id, entity.AdministrativeAreaId,
        entity.Name, entity.AddressLine, entity.Latitude, entity.Longitude, entity.PublicStatus, entity.Description);

    public static ProducerManagementResult Result(Producer entity) => new(entity.Id, entity.PublicStatus, entity.IsVerified,
        entity.ConcurrencyStamp);
}
