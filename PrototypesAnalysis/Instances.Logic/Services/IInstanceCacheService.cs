using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Instances.Logic.Models;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceCacheService : IService
	{
		Task<InstanceCacheObject> GetAsync(string code, bool includeData);
		Task SetAsync(InstanceCacheObject cacheObject);
		Task RemoveAsync(string code);
		Task TouchAsync(string code);
		Task<LockObject> LockAsync(string code);
		Task<string[]> GetInstancesInRangeAsync(double start = double.NegativeInfinity, double stop = double.PositiveInfinity);
    }
}
