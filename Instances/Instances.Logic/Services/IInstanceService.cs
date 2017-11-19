using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceService : IService
	{
		Task<InstanceCacheObject> CreateAsync(int prototype, int version);
		Task<InstanceCacheObject> PersistAsync(long companyId, string userCode, string code, Instance instance);
	}
}
