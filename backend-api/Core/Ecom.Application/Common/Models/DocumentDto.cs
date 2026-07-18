namespace Ecom.Application.Common.Models;

/// <summary>Public transport shape for a stored document; it intentionally has no persistence behavior.</summary>
public sealed class DocumentDto
{
    public string FileName { get; init; } = string.Empty;
    public string StorageKey { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
}
