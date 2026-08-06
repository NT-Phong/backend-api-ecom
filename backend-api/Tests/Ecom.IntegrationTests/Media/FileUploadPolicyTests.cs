using Ecom.Application.Common.Models;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Ecom.IntegrationTests.Media;

public class FileUploadPolicyTests
{
    private static readonly byte[] ValidPng = CreateValidPng();

    private static byte[] CreateValidPng()
    {
        using var image = new Image<Rgba32>(1, 1);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private readonly FileUploadPolicy _policy = new();

    [Fact]
    public async Task Product_image_accepts_matching_png_signature()
    {
        var bytes = ValidPng;
        await using var stream = new MemoryStream(bytes);

        var result = await _policy.ValidateAsync(stream, "product.png", "image/png", bytes.Length,
            MediaUploadIntent.ProductImage);

        Assert.Equal(".png", result.SafeExtension);
        Assert.Equal(Ecom.Domain.Enums.MediaVisibility.Public, result.TargetVisibility);
    }

    [Fact]
    public async Task Product_image_rejects_pdf_and_mime_spoofing()
    {
        var pdf = "%PDF-1.7 test"u8.ToArray();
        await using var pdfStream = new MemoryStream(pdf);
        await Assert.ThrowsAsync<InvalidDataException>(() => _policy.ValidateAsync(pdfStream,
            "product.pdf", "application/pdf", pdf.Length, MediaUploadIntent.ProductImage));

        var png = ValidPng;
        await using var spoofed = new MemoryStream(png);
        await Assert.ThrowsAsync<InvalidDataException>(() => _policy.ValidateAsync(spoofed,
            "product.jpg", "image/jpeg", png.Length, MediaUploadIntent.ProductImage));
    }

    [Fact]
    public async Task Upload_rejects_declared_size_that_differs_from_stream()
    {
        var png = ValidPng;
        await using var stream = new MemoryStream(png);

        await Assert.ThrowsAsync<InvalidDataException>(() => _policy.ValidateAsync(stream,
            "product.png", "image/png", png.Length + 1, MediaUploadIntent.ProductImage));
    }

    [Fact]
    public async Task Product_image_rejects_malformed_image_after_signature_validation()
    {
        var corruptPng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };
        await using var stream = new MemoryStream(corruptPng);

        await Assert.ThrowsAsync<InvalidDataException>(() => _policy.ValidateAsync(stream,
            "product.png", "image/png", corruptPng.Length, MediaUploadIntent.ProductImage));
    }
}
