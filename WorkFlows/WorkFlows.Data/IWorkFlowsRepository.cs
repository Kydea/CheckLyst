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
        Task<Common.Data.WorkFlows> GetWorkFlowAsync(DefaultIdentity identity, long id);
        Task<Common.Data.WorkFlows> AddWorkFlowAsync(DefaultIdentity identity, Common.Data.WorkFlows workFlow);
        Task<Common.Data.WorkFlows> EditWorkFlowAsync(DefaultIdentity identity, Common.Data.WorkFlows workFlow);
        Task DeleteWorkFlowAsync(DefaultIdentity identity, long id);
        Task<WorkFlowApplications> GetWorkFlowApplicationAsync(DefaultIdentity identity, long? prototype, int? version, long? instance);
        Task<WorkFlowApplicationSteps> GetWorkFlowApplicationStepAsync(DefaultIdentity identity, long id);
        Task<long> CreateWorkFlowApplication(DefaultIdentity defaultIdentity, long workFlow, DateTimeOffset? date, long? prototype, int? version, long? instance, string action);
        void DeletePrototypeVersionInstances(PrototypeVersions prototypeVersion);

    }
}