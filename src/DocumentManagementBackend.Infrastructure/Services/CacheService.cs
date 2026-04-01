using DocumentManagementBackend.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DocumentManagementBackend.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    // ⚠️ RemoveByPrefix funcționează doar cu Redis real, nu cu DistributedMemoryCache
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Cu Redis ai nevoie de IConnectionMultiplexer pentru pattern delete
        // Pentru acum invalidăm cheile cunoscute manual
        await _cache.RemoveAsync($"{prefix}:all", cancellationToken);
    }
}
