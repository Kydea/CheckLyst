using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceManagerService : IService
	{
		Task<InstanceCacheObject> GetAsync(long companyId, string code);
		Task<InstanceCacheObject> CreateAsync(long companyId, string userCode, long prototype, int version);
		Task OpenAsync(string userCode, string code);
		Task CloseAsync(long companyId, string userCode, string code);
		Task<InstanceCacheObject> HandleEventAsync(DefaultIdentity identity, InstanceEventRequest request);

		Instance GetCheckList(string userCode, bool canEdit, InstanceCacheObject instance);
	}
}
