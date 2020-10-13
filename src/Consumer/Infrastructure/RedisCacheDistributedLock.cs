using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading.Tasks;
using System;

namespace Consumer.Infrastructure
{
    public class RedisCacheDistributedLock : IDistributedLock
    {
        private readonly ILogger<RedisCacheDistributedLock> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private const string CachePrefix = "ConsumerLockedFile__";
        
        //
        // Because this lock is based on best effort and fence tokens,
        // if the node craches for any reason, we need to put an leasing expiration
        // otherwise deadlock. Tune this time based on your own needs
        //
        private const int DefaultExpirationLockTime = 3600;

        public RedisCacheDistributedLock(ILogger<RedisCacheDistributedLock> logger, ConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<bool> Lock(string resourceName, string fenceToken)
        {
            try
            {
                //
                // This is a best effort attemp to lock the resource
                //
                var lockName = $"{CachePrefix}{resourceName}";
                var lockedResource = await _database.StringGetAsync(lockName);
                if (lockedResource.IsNullOrEmpty)
                {
                    var created = await _database.StringSetAsync(lockName, fenceToken, TimeSpan.FromSeconds(DefaultExpirationLockTime));

                    return created;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "LockFailed");
            }

            return false;
        }

        public async Task<bool> Unlock(string resourceName, string fenceToken)
        {
            try
            {
                var lockName = $"{CachePrefix}{resourceName}";
                var lockedResource = await _database.StringGetAsync(lockName);

                //
                // Guarantee that the fence token is used, so only the node that claimed
                // the lock first can remove its lock
                //
                if (!lockedResource.IsNullOrEmpty && lockedResource == fenceToken)
                {
                    return await _database.KeyDeleteAsync($"{CachePrefix}{resourceName}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "UnlockFailed");
            }

            return false;
        }
    }
}