using System.Threading.Tasks;
public interface IDistributedLock
{
    Task<bool> Lock(string resourceName);
    Task<bool> Unlock(string resourceName);
}