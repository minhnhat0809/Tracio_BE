using StackExchange.Redis;
using System.Text.Json;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ContentService.Infrastructure.Redis
{
    public class CacheService(IDistributedCache cache, IConnectionMultiplexer  redisConnection)
        : ICacheService
    {
        private readonly IDatabase _cacheDb = redisConnection.GetDatabase();

        // Get item from cache
        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedData = await cache.GetStringAsync(key);
            return cachedData is null ? default : JsonSerializer.Deserialize<T>(cachedData);
        }

        // Set item in cache
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(expiration);

            var serializedData = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key, serializedData, options);
        }

        // Remove a specific item from cache
        public async Task RemoveAsync(string key)
        {
            await cache.RemoveAsync(key);
        }

        // Remove cache by pattern
        public async Task RemoveByPatternAsync(string pattern)
        {
            // Get the server instance
            var server = _cacheDb.Multiplexer.GetServer(_cacheDb.Multiplexer.GetEndPoints().First());

            // Get the keys that match the pattern
            var keys = server.Keys(pattern: pattern).ToArray();  // Returns an array of matching keys

            // Use batch processing to remove the keys efficiently
            if (keys.Length == 0)
            {
                return;
            }

            // Delete each key asynchronously
            foreach (var key in keys)
            {
                await _cacheDb.KeyDeleteAsync(key); 
            }
        }
    }
}
