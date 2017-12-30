using System.Threading;
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
		Task<InstanceCacheObject> GetAsync(IDefaultIdentity identity, string code);
		Task<InstanceCacheObject> CreateAsync(IDefaultIdentity identity, string userCode, long prototype, int version);
		Task OpenAsync(string code, IDefaultIdentity identity);
		Task SaveAsync(string code, IDefaultIdentity identity);
        Task CompleteAsync(string code, IDefaultIdentity identity);
        Task<InstanceCacheObject> HandleEventAsync(IDefaultIdentity identity, InstanceEventRequest request);
		Instance GetCheckList(IDefaultIdentity identity, string functionGroup, InstanceCacheObject instance);
		Task ClearPendingInstances(CancellationToken cancellationToken = default(CancellationToken));
	}
}
