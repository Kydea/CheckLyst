using CheckListManager.WorkFlows.Logic.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Tests.Data
{
    public interface IWorkFlowsRepositoryExtended
    {
        Task<List<ListRow>> GetRecordsAsync(long CompanyId, Expression<Func<Common.Data.WorkFlows, bool>> filters);
    }
}
