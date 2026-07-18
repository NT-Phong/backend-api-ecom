using Ecom.Domain.Enums;
using Ecom.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Ecom.IntegrationTests.Media;

public class LocalStorageServiceTests
{
    [Fact]
    public async Task Quarantine_can_be_promoted_but_only_public_media_gets_a_url()
    {
        var root = CreateRoot();
        try
        {
            var storage = new LocalStorageService(new TestEnvironment(root));
            await using var stream = new MemoryStream([1, 2, 3]);
            var quarantine = await storage.UploadToQuarantineAsync(stream, ".png");
            var publicKey = await storage.PromoteAsync(quarantine, MediaVisibility.Public);

            Assert.StartsWith("/media/", storage.GetPublicFileUrl(publicKey));
            Assert.Throws<InvalidOperationException>(() => storage.GetPublicFileUrl(quarantine));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task Storage_rejects_path_traversal()
    {
        var root = CreateRoot();
        try
        {
            var storage = new LocalStorageService(new TestEnvironment(root));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                storage.OpenReadAsync("uploads/../secret.txt"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "ecom-media-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Ecom.IntegrationTests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(root, "wwwroot");
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(root);
    }
}
