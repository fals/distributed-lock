using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;

namespace Consumer.Infrastructure
{
    public class LogWatcher : Watcher
    {
        private readonly ILogger<LogWatcher> _logger;

        public LogWatcher(ILogger<LogWatcher> logger)
        {
            _logger = logger;
        }

        public override Task process(WatchedEvent @event)
        {
            _logger.LogInformation("ZooKeeperEvent {0} {1}", @event.getState().ToString(), @event.getPath());

            return Task.CompletedTask;
        }
    }
}