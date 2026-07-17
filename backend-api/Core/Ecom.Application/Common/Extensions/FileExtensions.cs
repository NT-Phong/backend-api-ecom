namespace Ecom.Application.Common.Extensions;

public static class FileExtensions
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".rtf"
    };

    private static readonly HashSet<string> PdfExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4"
    };



    public static bool IsImageFile(this string fileName, string contentType)
        => IsInList(fileName, contentType, ImageExtensions, "image/");

    public static bool IsDocumentFile(this string fileName, string contentType)
        => IsInList(fileName, contentType, DocumentExtensions, "application/");

    public static bool IsPdfFile(this string fileName, string contentType)
        => IsInList(fileName, contentType, PdfExtensions, "application/pdf");

    public static bool IsVideoFile(this string fileName, string contentType)
        => IsInList(fileName, contentType, VideoExtensions, "video/");

    public static bool IsInList(
        string fileName,
        string contentType,
        HashSet<string> extensions,
        params string[] mimePrefixes
    )
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (mimePrefixes.Any(prefix => contentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return extensions.Contains(ext);
    }
}
