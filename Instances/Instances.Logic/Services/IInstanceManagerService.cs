using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Instances;
using CheckListManager.Common.Logic.Models;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceManagerService : IService
	{
        Task<InstanceCacheObject> GetCacheObjectAsync(long company, string code, bool includeData);
        Task SaveCacheObjectAsync(InstanceCacheObject cacheObject);
        Task GetInstanceAsync(IDefaultIdentity identity, InstanceCacheObject cacheObject);
        Task<InstanceCacheObject> CreateInstanceAsync(IDefaultIdentity identity, string userCode, long prototype, int? version, Dictionary<string, string> parameters);
		Task<InstanceCacheObject> OpenInstanceAsync(string code, IDefaultIdentity identity);
        Task SaveInstanceAsync(string code, IDefaultIdentity identity);
        Task<InstanceCacheObject> CompleteInstanceAsync(string code, IDefaultIdentity identity);
        Task DeleteInstanceAsync(string code, IDefaultIdentity identity, string note);
        Task<Instance> UpdateInstanceAsync(IDefaultIdentity identity, InstanceCacheObject instance, string lockUser);
        Task<InstanceCacheObject> HandleEventAsync(IDefaultIdentity identity, InstanceEventRequest request);
        Task ClearPendingInstances(CancellationToken cancellationToken = default(CancellationToken));
        Task<List<HistoryRow>> History(DefaultIdentity identity, long id);
        Task<string> GetExternalSourceRows(string code, IDefaultIdentity identity, string dataSourceCode, Dictionary<string, string> parameters, int page);
        Task<List<InstanceEventRequest.Email>> GetInstanceEditors(string code, IDefaultIdentity identity);
        Task<StorageFile> DownloadAttachmentAsync(IDefaultIdentity identity, string cacheObjectCode, string key);
        Task InvalidateCacheAsync(DefaultIdentity identity, InvalidateCacheRequest request);
        void CheckViewPermission(IDefaultIdentity identity, InstanceCacheObject cacheObject);
    }
}
