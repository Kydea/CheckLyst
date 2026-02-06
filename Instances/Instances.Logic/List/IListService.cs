using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Instances.Data.Models;
using CheckListManager.Instances.Logic.Messages;

namespace CheckListManager.Instances.Logic.List
{
    public interface IListService: IService
    {
        Task<ListResponse> BindAsync(IDefaultIdentity identity, InstanceFilters form);
        Task<PagingResponse<List<InstanceRecord>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<InstanceFilters> form);
        Task<OverviewResponse> OverviewAsync(IDefaultIdentity identity, InstanceFilters filters);
        Task<LifeCycleResponse> LifeCycleAsync(IDefaultIdentity identity, InstanceFilters filters);
        Task<TasksResponse> GetTasksAsync(IDefaultIdentity identity, string timeZone, InstanceFilters filters);
        Task<long> CountTasksAsync(IDefaultIdentity identity);
        Task<List<InstanceRecord>> ExportAsync(InstanceFilters form);
    }
}
