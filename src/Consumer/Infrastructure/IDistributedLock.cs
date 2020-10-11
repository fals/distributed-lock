using System.Threading.Tasks;

namespace Consumer.Infrastructure
{
    public interface IDistributedLock
    {
        Task<bool> Lock(string resourceName);
        Task<bool> Unlock(string resourceName);
    }
}