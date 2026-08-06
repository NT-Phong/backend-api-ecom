using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Media.Commands.DeleteMedia;

public sealed record DeleteMediaCommand(Guid MediaAssetId) : IRequest<TResult>, ITransactionalRequest;

public sealed class DeleteMediaCommandHandler(IUnitOfWork unitOfWork, IMediaAccessService access)
    : IRequestHandler<DeleteMediaCommand, TResult>
{
    public async Task<TResult> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var media = await unitOfWork.Repository<MediaAsset>().FindByIdAsync(request.MediaAssetId);
        if (media is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var authorization = access.EnsureOwnerOrManager(media);
        if (!authorization.IsSuccess) return authorization;
        if (await unitOfWork.Repository<ProductMedia>().AnyAsync([x => x.MediaAssetId == media.Id]))
            return TResult.Failure("MEDIA_IN_USE", ErrorCodes.ALREADY_EXISTS);
        await unitOfWork.Repository<MediaAsset>().DeleteAsync(media, cancellationToken);
        return TResult.Success();
    }
}
