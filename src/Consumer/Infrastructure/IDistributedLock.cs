using System.Threading.Tasks;

namespace Consumer.Infrastructure
{
    public interface IDistributedLock
    {
        Task<bool> Lock(string resourceName, string nodeName);
        Task<bool> Unlock(string resourceName, string nodeName);
    }
}