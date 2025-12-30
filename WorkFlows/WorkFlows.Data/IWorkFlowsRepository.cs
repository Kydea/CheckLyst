using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.WorkFlows.Data
{
    public interface IWorkFlowsRepository : IRepository
    {
        Task<PagingResponse<List<TResult>>> GetWorkFlowsAsync<TResult, TKey>(long companyId, Expression<Func<Common.Data.WorkFlows, TResult>> select, Expression<Func<Common.Data.WorkFlows, bool>> filters, Expression<Func<Common.Data.WorkFlows, TKey>> order, int currentPage, int itemsPerPage);
        Task<Common.Data.WorkFlows> GetWorkFlowAsync(IDefaultIdentity identity, long id);
        Task<Common.Data.WorkFlows> AddWorkFlowAsync(IDefaultIdentity identity, Common.Data.WorkFlows workFlow);
        Task<Common.Data.WorkFlows> EditWorkFlowAsync(IDefaultIdentity identity, Common.Data.WorkFlows workFlow);
        Task DeleteWorkFlowAsync(IDefaultIdentity identity, long id);
        Task<WorkFlowApplications> GetWorkFlowApplicationAsync(IDefaultIdentity identity, long? prototype, int? version, long? instance, long? report);
        Task<WorkFlowApplicationSteps> GetWorkFlowApplicationStepAsync(IDefaultIdentity identity, long id);
        Task<WorkFlowApplications> CreateWorkFlowApplication(IDefaultIdentity defaultIdentity, long workFlow, DateTimeOffset? date, long? prototype, int? version, long? instance, long? report, string action);
        void DeletePrototypeVersionInstances(PrototypeVersions prototypeVersion);

    }
}