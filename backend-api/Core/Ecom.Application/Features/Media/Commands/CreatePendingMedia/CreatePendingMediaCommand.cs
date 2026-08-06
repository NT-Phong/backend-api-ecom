using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Media.Commands.CreatePendingMedia;

public sealed record CreatePendingMediaCommand(string StorageKey, ValidatedMediaUpload Upload,
    MediaUploadIntent Intent, string? AltText) : IRequest<TResult<MediaAssetResult>>, ITransactionalRequest;

public sealed class CreatePendingMediaCommandValidator : AbstractValidator<CreatePendingMediaCommand>
{
    public CreatePendingMediaCommandValidator()
    {
        RuleFor(x => x.StorageKey).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Intent).Equal(MediaUploadIntent.ProductImage);
        RuleFor(x => x.AltText).MaximumLength(500);
    }
}

public sealed class CreatePendingMediaCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<CreatePendingMediaCommand, TResult<MediaAssetResult>>
{
    public async Task<TResult<MediaAssetResult>> Handle(CreatePendingMediaCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.HasPolicy(Permissions.Media.Upload))
            return TResult<MediaAssetResult>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        if (request.Intent != MediaUploadIntent.ProductImage || request.Upload.MediaType != MediaType.Image ||
            request.Upload.TargetVisibility != MediaVisibility.Public)
            return TResult<MediaAssetResult>.Failure(MessageKey.ValidationFailed, ErrorCodes.BAD_REQUEST);

        var media = MediaAsset.CreatePending(request.StorageKey, request.Upload.OriginalFileName,
            request.Upload.ContentType, request.Upload.SizeBytes, request.Upload.MediaType,
            MediaVisibility.Restricted, request.Intent, request.Upload.TargetVisibility, request.AltText);
        await unitOfWork.Repository<MediaAsset>().InsertAsync(media, cancellationToken);
        return TResult<MediaAssetResult>.Success(new MediaAssetResult(media.Id, media.OriginalFileName,
            media.ContentType, media.SizeBytes, media.MediaType, media.Visibility, media.ScanStatus,
            media.TargetVisibility));
    }
}
