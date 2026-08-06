using Ecom.Application.Common.Commerce;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database;

namespace Ecom.Infrastructure.Services;

public sealed class IdempotencyStore(ApplicationDbContext db) : IIdempotencyStore
{
    public async Task<IdempotencyBeginResult> BeginAsync(string operation, string ownerScope, string key,
        string fingerprint, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var keyHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
        var id = Guid.NewGuid();
        var stamp = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE ""Tbl_IdempotencyRecord"" SET ""IsDeleted"" = true, ""DeletedAt"" = {now}
WHERE ""Operation"" = {operation} AND ""OwnerScope"" = {ownerScope} AND ""KeyHash"" = {keyHash}
  AND ""ExpiresAt"" <= {now} AND ""Status"" = {IdempotencyStatus.Completed.ToString()} AND ""IsDeleted"" = false;", cancellationToken);
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO ""Tbl_IdempotencyRecord"" (""Id"", ""ConcurrencyStamp"", ""Operation"", ""OwnerScope"", ""KeyHash"", ""RequestFingerprint"", ""Status"", ""ExpiresAt"", ""CreatedAt"", ""IsDeleted"")
VALUES ({id}, {stamp}, {operation}, {ownerScope}, {keyHash}, {fingerprint}, {IdempotencyStatus.Processing.ToString()}, {expiresAt}, {now}, false)
ON CONFLICT (""Operation"", ""OwnerScope"", ""KeyHash"") WHERE ""IsDeleted"" = false DO NOTHING;", cancellationToken);

        var record = await db.IdempotencyRecords.SingleAsync(x => x.Operation == operation && x.OwnerScope == ownerScope && x.KeyHash == keyHash, cancellationToken);
        if (inserted == 1)
            return new IdempotencyBeginResult(IdempotencyBeginKind.Started, record);
        if (!string.Equals(record.RequestFingerprint, fingerprint, StringComparison.Ordinal))
            return new IdempotencyBeginResult(IdempotencyBeginKind.Mismatch, record);
        return new IdempotencyBeginResult(record.Status == IdempotencyStatus.Completed
            ? IdempotencyBeginKind.Completed : IdempotencyBeginKind.Processing, record);
    }
}
