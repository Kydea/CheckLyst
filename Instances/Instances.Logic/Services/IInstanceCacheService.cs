using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Instances.Logic.Models;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceCacheService : IService
	{
		Task<InstanceCacheObject> GetAsync(string code, bool includeData = true);
		Task SetAsync(string code, InstanceCacheObject instance);
		Task RemoveAsync(string code);

		Task TouchInstanceAsync(string code, double? score = null);

		//Task<IEnumerable<InstanceCacheObject>> GetByUserAsync(string userCode);
	}
}
