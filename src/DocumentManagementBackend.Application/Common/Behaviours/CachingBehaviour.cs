using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Application.Common.Behaviors;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
            return await next();

        // ✅ Cache Hit — caută în Redis
        var cached = await _cache.GetStringAsync(cacheableQuery.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheableQuery.CacheKey);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        // ✅ Cache Miss — execută query și stochează în Redis
        var response = await next();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheableQuery.CacheDuration
        };

        await _cache.SetStringAsync(
            cacheableQuery.CacheKey,
            JsonSerializer.Serialize(response),
            options,
            cancellationToken);

        _logger.LogInformation("Cache miss — stored {CacheKey}", cacheableQuery.CacheKey);
        return response;
    }
}
