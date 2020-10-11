using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;
using System;

public class RedisCacheDistributedLock : IDistributedLock
{
    private readonly ILogger<RedisCacheDistributedLock> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private const string CachePrefix = "ConsumerLockedFile__";

    public RedisCacheDistributedLock(ILogger<RedisCacheDistributedLock> logger, ConnectionMultiplexer redis, IDatabase database)
    {
        _logger = logger;
        _redis = redis;
        _database = database;
    }

    public async Task<bool> Lock(string resourceName)
    {
        var lockName = $"{CachePrefix}{resourceName}";
        var lockedResource = await _database.StringGetAsync(lockName);

        if (lockedResource.IsNullOrEmpty)
        {
            var created = await _database.StringSetAsync(lockName, Environment.MachineName);

            return created;
        }

        return false;
    }

    public async Task<bool> Unlock(string resourceName)
    {
        return await _database.KeyDeleteAsync($"{CachePrefix}{resourceName}");
    }
}