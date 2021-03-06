using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using static org.apache.zookeeper.ZooDefs;

namespace Consumer.Infrastructure
{
    public class ZooKeeperDistributedLock : IDistributedLock
    {
        private readonly ILogger<ZooKeeperDistributedLock> _logger;
        private readonly ZooKeeper _zooKeeper;

        public ZooKeeperDistributedLock(ILogger<ZooKeeperDistributedLock> logger, ZooKeeper zooKeeper)
        {
            _logger = logger;
            _zooKeeper = zooKeeper;
        }

        public async Task<bool> Lock(string resourceName, string fenceToken)
        {
            try
            {
                var lockName = $"/consumer/lock-{resourceName.Replace(@"/", "_")}";
                var data = Encoding.UTF8.GetBytes(fenceToken);
                await _zooKeeper.createAsync(lockName, data, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                await _zooKeeper.getChildrenAsync(lockName);
                var exists = await _zooKeeper.existsAsync(lockName);

                return exists != null;
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
                var lockName = $"/consumer/lock-{resourceName.Replace(@"/", "_")}";

                await _zooKeeper.deleteAsync(lockName);

                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "UnlockFailed");
            }
            
            return false;
        }
    }
}