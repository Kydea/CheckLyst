using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;

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
    }
}
