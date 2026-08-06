using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Domain.Enums;
using SixLabors.ImageSharp;

namespace Ecom.Infrastructure.Services;

public sealed class FileUploadPolicy : IFileUploadPolicy
{
    private const long TenMegabytes = 10 * 1024 * 1024;
    private const long TwentyMegabytes = 20 * 1024 * 1024;

    public async Task<ValidatedMediaUpload> ValidateAsync(Stream stream, string fileName, string claimedContentType,
        long sizeBytes, MediaUploadIntent intent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new InvalidOperationException("Upload stream must be readable and seekable.");
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(claimedContentType))
            throw new InvalidDataException("File name and content type are required.");

        var maxSize = intent == MediaUploadIntent.TradeInquiryAttachment ? TwentyMegabytes : TenMegabytes;
        if (sizeBytes <= 0 || sizeBytes > maxSize || stream.Length != sizeBytes)
            throw new InvalidDataException($"File size must be between 1 byte and {maxSize} bytes and match the stream length.");

        var originalPosition = stream.Position;
        stream.Position = 0;
        var header = new byte[12];
        var read = 0;
        while (read < header.Length)
        {
            var current = await stream.ReadAsync(header.AsMemory(read, header.Length - read), cancellationToken);
            if (current == 0) break;
            read += current;
        }
        stream.Position = originalPosition;

        var detected = Detect(header.AsSpan(0, read));
        var claimed = claimedContentType.Trim().ToLowerInvariant();
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var extensionMatches = extension == detected.Extension ||
                               (detected.Extension == ".jpg" && extension == ".jpeg");
        if (!string.Equals(claimed, detected.ContentType, StringComparison.Ordinal) || !extensionMatches)
            throw new InvalidDataException("File extension, declared content type, and file signature do not match.");

        EnsureAllowed(intent, detected.MediaType, detected.ContentType);
        if (detected.MediaType == MediaType.Image)
            await EnsureSafeImageAsync(stream, originalPosition, cancellationToken);
        var visibility = intent == MediaUploadIntent.ProductImage ? MediaVisibility.Public : MediaVisibility.Restricted;
        return new ValidatedMediaUpload(Path.GetFileName(fileName), detected.ContentType, detected.Extension,
            sizeBytes, detected.MediaType, visibility);
    }

    private static async Task EnsureSafeImageAsync(Stream stream, long originalPosition, CancellationToken cancellationToken)
    {
        stream.Position = 0;
        try
        {
            var info = await Image.IdentifyAsync(stream, cancellationToken);
            if (info is null || info.Width <= 0 || info.Height <= 0 || (long)info.Width * info.Height > 40_000_000)
                throw new InvalidDataException("Image dimensions exceed the upload safety limit.");
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
        {
            throw new InvalidDataException("Image content cannot be decoded.", ex);
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static (string ContentType, string Extension, MediaType MediaType) Detect(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 4 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ("image/jpeg", ".jpg", MediaType.Image);
        if (header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ("image/png", ".png", MediaType.Image);
        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8))
            return ("image/webp", ".webp", MediaType.Image);
        if (header.Length >= 5 && header[..5].SequenceEqual("%PDF-"u8))
            return ("application/pdf", ".pdf", MediaType.Document);
        throw new InvalidDataException("Unsupported or unrecognized file signature.");
    }

    private static void EnsureAllowed(MediaUploadIntent intent, MediaType mediaType, string contentType)
    {
        if (intent == MediaUploadIntent.ProductImage && mediaType != MediaType.Image)
            throw new InvalidDataException("Product media only supports JPEG, PNG, or WebP images.");
        if (intent is MediaUploadIntent.TradeInquiryAttachment or MediaUploadIntent.BankTransferProof &&
            mediaType is not (MediaType.Image or MediaType.Document))
            throw new InvalidDataException($"The media type {contentType} is not allowed for this upload.");
    }
}
