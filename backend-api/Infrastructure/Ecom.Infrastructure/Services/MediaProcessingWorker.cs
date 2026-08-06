using System.Security.Cryptography;
using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Ecom.Infrastructure.Services;

public sealed class MediaProcessingWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<MediaProcessingOptions> options,
    ILogger<MediaProcessingWorker> logger) : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays =
    [TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30), TimeSpan.FromHours(2), TimeSpan.FromHours(12)];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Media processing worker is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Media processing batch failed.");
            }
            await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(options.Value.PollSeconds, 5, 300)), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var candidates = await unitOfWork.Repository<MediaAsset>().FindAsync(
            [x => x.ScanStatus == MediaScanStatus.Pending && x.UploadIntent == MediaUploadIntent.ProductImage &&
                  (x.NextScanAttemptAt == null || x.NextScanAttemptAt <= DateTime.UtcNow) &&
                  (x.ScanLeaseExpiresAt == null || x.ScanLeaseExpiresAt <= DateTime.UtcNow)],
            orderBy: "CreatedAt", limit: 20);
        foreach (var candidate in candidates)
        {
            if (!await TryClaimAsync(candidate.Id, cancellationToken)) continue;
            await ProcessOneAsync(candidate.Id, cancellationToken);
        }

        await CleanupExpiredAsync(cancellationToken);
    }

    private async Task<bool> TryClaimAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;
        var claimed = await unitOfWork.Repository<MediaAsset>().Query()
            .Where(x => x.Id == mediaId &&
                        x.ScanStatus == MediaScanStatus.Pending &&
                        (x.NextScanAttemptAt == null || x.NextScanAttemptAt <= now) &&
                        (x.ScanLeaseExpiresAt == null || x.ScanLeaseExpiresAt <= now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ScanLeaseExpiresAt, now.AddMinutes(5)), cancellationToken);
        return claimed == 1;
    }

    private async Task ProcessOneAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();
        var scanner = scope.ServiceProvider.GetRequiredService<IMalwareScanner>();
        var thumbnails = scope.ServiceProvider.GetRequiredService<IDocumentService>();
        var media = await unitOfWork.Repository<MediaAsset>().FindByIdAsync(mediaId);
        if (media is null || media.ScanStatus != MediaScanStatus.Pending) return;

        string? promotedOriginal = null;
        string? quarantineThumbnail = null;
        string? promotedThumbnail = null;
        try
        {
            await using (var scanStream = await storage.OpenReadAsync(media.StorageKey, cancellationToken))
            {
                if (!await scanner.IsCleanAsync(scanStream, cancellationToken))
                {
                    media.Reject("Malware scan rejected the upload.", DateTime.UtcNow);
                    await unitOfWork.Repository<MediaAsset>().UpdateAsync(media, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    return;
                }
            }

            await using (var hashStream = await storage.OpenReadAsync(media.StorageKey, cancellationToken))
                media.SetSha256(Convert.ToHexString(await SHA256.HashDataAsync(hashStream, cancellationToken)).ToLowerInvariant());

            await using (var imageStream = await storage.OpenReadAsync(media.StorageKey, cancellationToken))
            await using (var thumbnail = await thumbnails.CreateWebPThumbnailAsync(imageStream, cancellationToken: cancellationToken))
            {
                if (thumbnail is null) throw new InvalidDataException("A ProductImage must generate a thumbnail.");
                quarantineThumbnail = await storage.UploadToQuarantineAsync(thumbnail, ".webp", "image/webp", cancellationToken);
            }

            promotedOriginal = await storage.PromoteAsync(media.StorageKey, media.TargetVisibility, cancellationToken);
            promotedThumbnail = await storage.PromoteAsync(quarantineThumbnail!, media.TargetVisibility, cancellationToken);
            var quarantineOriginal = media.StorageKey;
            media.MarkClean(promotedOriginal, media.TargetVisibility, DateTime.UtcNow);
            media.SetThumbnailStorageKey(promotedThumbnail);
            await unitOfWork.Repository<MediaAsset>().UpdateAsync(media, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await storage.DeleteIfExistsAsync(quarantineOriginal, cancellationToken);
            await storage.DeleteIfExistsAsync(quarantineThumbnail!, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (promotedOriginal is not null) await storage.DeleteIfExistsAsync(promotedOriginal, cancellationToken);
            if (promotedThumbnail is not null) await storage.DeleteIfExistsAsync(promotedThumbnail, cancellationToken);
            if (media.ScanAttemptCount >= RetryDelays.Length - 1)
                media.MarkScanFailed("Media processing failed.", DateTime.UtcNow);
            else
                media.ScheduleScanRetry(DateTime.UtcNow.Add(RetryDelays[media.ScanAttemptCount]));
            await unitOfWork.Repository<MediaAsset>().UpdateAsync(media, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogWarning(ex, "Media processing failed for {MediaAssetId}.", mediaId);
        }
    }

    private async Task CleanupExpiredAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();
        var now = DateTime.UtcNow;
        var pendingExpiry = now.AddHours(-24);
        var failedExpiry = now.AddDays(-7);
        var tombstoneExpiry = now.AddDays(-30);
        var repository = unitOfWork.Repository<MediaAsset>();
        var candidates = await repository.Query(includeDeleted: true)
            .Where(x => x.IsDeleted ||
                        (x.ScanStatus == MediaScanStatus.Pending && x.CreatedAt <= pendingExpiry) ||
                        ((x.ScanStatus == MediaScanStatus.Rejected || x.ScanStatus == MediaScanStatus.Failed) &&
                         x.CreatedAt <= failedExpiry))
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var media in candidates)
        {
            try
            {
                await storage.DeleteIfExistsAsync(media.StorageKey, cancellationToken);
                if (!string.IsNullOrWhiteSpace(media.ThumbnailStorageKey))
                    await storage.DeleteIfExistsAsync(media.ThumbnailStorageKey, cancellationToken);

                if (media.IsDeleted && media.DeletedAt <= tombstoneExpiry)
                    await repository.HardDeleteAsync(media.Id, cancellationToken);
                else if (!media.IsDeleted)
                    await repository.DeleteAsync(media, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Media cleanup failed for {MediaAssetId}.", media.Id);
            }
        }
    }
}
