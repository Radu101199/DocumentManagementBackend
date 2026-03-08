using MediatR;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
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

        if (_cache.TryGetValue(cacheableQuery.CacheKey, out TResponse? cachedResponse))
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheableQuery.CacheKey);
            return cachedResponse!;
        }

        var response = await next();

        _cache.Set(cacheableQuery.CacheKey, response,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheableQuery.CacheDuration
            });

        _logger.LogInformation("Cache miss — stored {CacheKey}", cacheableQuery.CacheKey);

        return response;
    }
}