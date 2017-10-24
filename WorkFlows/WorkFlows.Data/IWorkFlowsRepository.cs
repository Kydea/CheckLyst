using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.WorkFlows.Data
{
    public interface IWorkFlowsRepository : IRepository
    {
        Task<PagingResponse<List<TResult>>> GetWorkFlowsAsync<TResult, TKey>(long companyId, Expression<Func<Common.Data.WorkFlows, TResult>> select, Expression<Func<Common.Data.WorkFlows, bool>> filters, Expression<Func<Common.Data.WorkFlows, TKey>> order, int currentPage, int itemsPerPage);
    }
}