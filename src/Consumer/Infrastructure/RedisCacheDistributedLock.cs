using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading.Tasks;
using System;

public class RedisCacheDistributedLock : IDistributedLock
{
    private readonly ILogger<RedisCacheDistributedLock> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private const string CachePrefix = "ConsumerLockedFile__";
    private const int DefaultExpirationLockTime = 3600;

    public RedisCacheDistributedLock(ILogger<RedisCacheDistributedLock> logger, ConnectionMultiplexer redis, IDatabase database)
    {
        _logger = logger;
        _redis = redis;
        _database = database;
    }

    public async Task<bool> Lock(string resourceName)
    {
        try
        {
            var lockName = $"{CachePrefix}{resourceName}";
            var lockedResource = await _database.StringGetAsync(lockName);
            var lockHolder = Environment.MachineName;

            if (lockedResource.IsNullOrEmpty)
            {
                var created = await _database.StringSetAsync(lockName, lockHolder, TimeSpan.FromSeconds(DefaultExpirationLockTime));

                return created;
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError("LockFailed", ex);
        }

        return false;
    }

    public async Task<bool> Unlock(string resourceName)
    {
        try
        {
            var lockName = $"{CachePrefix}{resourceName}";
            var lockedResource = await _database.StringGetAsync(lockName);
            var lockHolder = Environment.MachineName;

            if (lockedResource.IsNullOrEmpty && lockedResource == lockHolder)
            {
                return await _database.KeyDeleteAsync($"{CachePrefix}{resourceName}");
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError("UnlockFailed", ex);
        }

        return false;
    }
}