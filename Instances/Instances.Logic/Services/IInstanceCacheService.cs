using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Instances.Logic.Models;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceCacheService : IService
	{
		Task<InstanceCacheObject> GetAsync(string code, bool includeData = true);
		//Task<IEnumerable<InstanceCacheObject>> GetByUserAsync(string userCode);

		Task SetAsync(string code, InstanceCacheObject instance);
		//Task RemoveAsync(string code, string userCode);
	}
}
