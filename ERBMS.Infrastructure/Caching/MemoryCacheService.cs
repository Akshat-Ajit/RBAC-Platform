using Microsoft.Extensions.Caching.Memory;

namespace ERBMS.Infrastructure.Caching;

public class MemoryCacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        _cache.Set(key, value, ttl);
        return Task.CompletedTask;
    }
}
