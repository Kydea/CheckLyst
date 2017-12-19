using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.WorkFlows.Data
{
    public class WorkFlowsRepository : Repository, IWorkFlowsRepository
    {
        public WorkFlowsRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public async Task<PagingResponse<List<TResult>>> GetWorkFlowsAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Common.Data.WorkFlows, TResult>> select,
            Expression<Func<Common.Data.WorkFlows, bool>> filters,
            Expression<Func<Common.Data.WorkFlows, TKey>> order, 
            int currentPage, 
            int itemsPerPage)
        {
            var Data = await Entities.WorkFlows
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            int Count = await Entities.WorkFlows
                .Where(filters)
                .CountAsync();

            return new PagingResponse<List<TResult>>
            {
                Data = Data,
                Count = Count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }
        public async Task<Common.Data.WorkFlows> GetWorkFlowAsync(DefaultIdentity identity, long id)
        {
            var WorkFlow = await Entities.WorkFlows.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id);
            if (WorkFlow == null) throw new DatabaseException($"Work flow {id} not found");
            return WorkFlow;
        }

        public async Task<Common.Data.WorkFlows> AddWorkFlowAsync(DefaultIdentity identity, Common.Data.WorkFlows workFlow)
        {
            workFlow.Company = identity.CompanyId;
            workFlow.CreatedOn = DateTimeOffset.UtcNow;
            workFlow.CreatedBy = identity.Code;
            Entities.WorkFlows.Add(workFlow);
            await SaveChangesAsync();
            return workFlow;
        }

        public async Task<Common.Data.WorkFlows> EditWorkFlowAsync(DefaultIdentity identity, Common.Data.WorkFlows workFlow)
        {
            var ExistingPrototypeType = await GetWorkFlowAsync(identity, workFlow.ID);
            ExistingPrototypeType.Description = workFlow.Description;
            ExistingPrototypeType.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingPrototypeType.ModifiedBy = identity.Code;
            Entities.WorkFlowSteps.RemoveRange(ExistingPrototypeType.WorkFlowSteps);
            ExistingPrototypeType.WorkFlowSteps = workFlow.WorkFlowSteps;
            await SaveChangesAsync();
            return ExistingPrototypeType;
        }

        public async Task DeleteWorkFlowAsync(DefaultIdentity identity, long id)
        {
            Entities.WorkFlows.Remove(await GetWorkFlowAsync(identity, id));
            await SaveChangesAsync();
        }

        public async Task<WorkFlowApplications> GetWorkFlowApplicationAsync(DefaultIdentity identity, long? prototype, int? version, long? instance)
        {
            var Application = await Entities.WorkFlowApplications.SingleOrDefaultAsync(x => (x.Prototype == prototype && x.Version == version || x.Instance == instance) && x.Approved == null);
            if (Application == null) throw new ArgumentException($"Workflow application for {(instance == null ? $"prototype version {prototype}.{version}" : $"instance {instance}")} not found");
            return Application;
        }
    }
}
