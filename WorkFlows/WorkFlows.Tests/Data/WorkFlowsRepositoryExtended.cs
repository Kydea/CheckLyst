using CheckListManager.Common.Data;
using CheckListManager.WorkFlows.Logic.List;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Tests.Data
{
    public class WorkFlowsRepositoryExtended : Repository, IWorkFlowsRepositoryExtended
    {
        public WorkFlowsRepositoryExtended(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public Task<List<ListRow>> GetRecordsAsync(long CompanyId, Expression<Func<Common.Data.WorkFlows, bool>> filters) 
            => Entities.WorkFlows.Where(wf => wf.Company == CompanyId).Where(filters).Select(wf => new ListRow {
                    ID = wf.ID,
                    Description = wf.Description,
                    CreatedBy = wf.CreatedBy,
                    CreatedOn = wf.CreatedOn,
                    ModifiedBy = wf.ModifiedBy,
                    ModifiedOn = wf.ModifiedOn
                }).ToListAsync();
    }
}
