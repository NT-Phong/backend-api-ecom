using Ecom.Application.Features.Demo.QrLogin;
using Ecom.Infrastructure.Caching;
using Ecom.Infrastructure.Locking;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Ecom.IntegrationTests.Auth;

public sealed class DemoQrLoginStoreTests
{
    [Fact]
    public async Task Approve_is_single_use_and_keeps_the_approving_user_internal()
    {
        var store = CreateStore();
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await store.CreateAsync(new DemoQrLoginAttempt(id, DemoQrLoginStatus.Pending, now.AddMinutes(2), null, null));

        var first = await store.TryTransitionAsync(id, DemoQrLoginStatus.Approved, userId, now);
        var second = await store.TryTransitionAsync(id, DemoQrLoginStatus.Rejected, Guid.NewGuid(), now.AddSeconds(1));
        var stored = await store.GetAsync(id);

        Assert.Equal(DemoQrLoginTransitionResult.Updated, first);
        Assert.Equal(DemoQrLoginTransitionResult.AlreadyCompleted, second);
        Assert.NotNull(stored);
        Assert.Equal(DemoQrLoginStatus.Approved, stored!.Status);
        Assert.Equal(userId, stored.ApprovedUserId);
    }

    [Fact]
    public async Task Expired_attempt_cannot_be_approved()
    {
        var store = CreateStore();
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        await store.CreateAsync(new DemoQrLoginAttempt(id, DemoQrLoginStatus.Pending, now.AddMinutes(1), null, null));

        var result = await store.TryTransitionAsync(id, DemoQrLoginStatus.Approved, Guid.NewGuid(), now.AddMinutes(2));

        Assert.Equal(DemoQrLoginTransitionResult.MissingOrExpired, result);
    }

    private static DemoQrLoginStore CreateStore() => new(
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
        new InMemoryDistributedLockService(),
        NullLogger<DemoQrLoginStore>.Instance);
}
