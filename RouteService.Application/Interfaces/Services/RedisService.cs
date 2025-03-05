using Microsoft.EntityFrameworkCore.Storage;

namespace RouteService.Application.Interfaces.Services;

using StackExchange.Redis;

public class RedisService
{
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task SetValueAsync(string key, string value)
    {
        await _database.StringSetAsync(key, value);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        return await _database.StringGetAsync(key);
    }
}
