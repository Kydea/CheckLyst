using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using System.Collections.Generic;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceManagerService : IService
	{
		Task<InstanceCacheObject> GetAsync(long company, string userCode, string code);
		Task<InstanceCacheObject> Open(IDefaultIdentity identity, string code);
        Task<InstanceCacheObject> GetAsync(long company, long ID);
        Task<InstanceCacheObject> CreateAsync(IDefaultIdentity identity, string userCode, long prototype, int? version);
		Task<InstanceCacheObject> OpenAsync(string code, IDefaultIdentity identity);
        Task SaveAsync(string code, IDefaultIdentity identity);
        Task<InstanceCacheObject> CompleteAsync(string code, IDefaultIdentity identity);
        Task DeleteAsync(string code, IDefaultIdentity identity, string note);
        Task<InstanceCacheObject> HandleEventAsync(IDefaultIdentity identity, InstanceEventRequest request);
		Task<Instance> GetCheckListAsync(IDefaultIdentity identity, InstanceCacheObject instance, string lockUser);
		Task ClearPendingInstances(CancellationToken cancellationToken = default(CancellationToken));
        Task<List<HistoryRow>> History(DefaultIdentity identity, long id);
        Task<string> GetExternalSourceRows(string code, IDefaultIdentity identity, string dataSourceCode, Dictionary<string, string> parameters, int page);
    }
}
