using Ecom.Application.Common.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Ecom.Infrastructure.Services;

public sealed class DocumentService : IDocumentService
{
    public async Task<Stream?> CreateWebPThumbnailAsync(Stream originalStream, int thumbnailWidth = 320, int thumbnailQuality = 75, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(originalStream);
        if (thumbnailWidth <= 0 || thumbnailQuality is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(thumbnailWidth));
        try
        {
            if (!originalStream.CanSeek)
                throw new InvalidOperationException("Thumbnail source stream must be seekable.");
            var position = originalStream.Position;
            var info = await Image.IdentifyAsync(originalStream, cancellationToken);
            originalStream.Position = position;
            if ((long)info.Width * info.Height > 40_000_000)
                throw new InvalidDataException("Image dimensions exceed the thumbnail safety limit.");
            using var image = await Image.LoadAsync(originalStream, cancellationToken);
            image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(thumbnailWidth, thumbnailWidth) }));
            var output = new MemoryStream();
            await image.SaveAsync(output, new WebpEncoder { Quality = thumbnailQuality }, cancellationToken);
            output.Position = 0;
            return output;
        }
        catch (UnknownImageFormatException)
        {
            return null;
        }
    }
}
