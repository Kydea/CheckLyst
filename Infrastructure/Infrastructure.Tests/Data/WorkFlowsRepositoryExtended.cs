using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests.Data
{
    public class WorkFlowsRepositoryExtended : Repository, IWorkFlowsRepositoryExtended
    {
        public WorkFlowsRepositoryExtended(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public async Task<WorkFlows> AddWorkFlowAsync(IDefaultIdentity identity, WorkFlows workFlow)
        {
            workFlow.Company = identity.CompanyId;
            workFlow.CreatedOn = DateTimeOffset.UtcNow;
            workFlow.CreatedBy = identity.Code;
            Entities.WorkFlows.Add(workFlow);
            await SaveChangesAsync();
            return workFlow;
        }
    }
}
