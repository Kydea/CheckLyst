using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceManagerService : IService
	{
		Task<InstanceCacheObject> GetAsync(long companyId, string code);
		Task<InstanceCacheObject> CreateAsync(long companyId, string userCode, int prototype, int version);
		Task OpenAsync(string userCode, string code);
		Task CloseAsync(long companyId, string userCode, string code);

		Instance GetCheckList(string userCode, bool canEdit, InstanceCacheObject instance);
	}
}
