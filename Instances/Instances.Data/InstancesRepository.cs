using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.Instances.Data
{
    public class InstancesRepository : Repository, IInstancesRepository
    {
        public InstancesRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public async Task<PagingResponse<List<TResult>>> GetInstancessAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Common.Data.Instances, TResult>> select,
            Expression<Func<Common.Data.Instances, bool>> filters,
            Expression<Func<Common.Data.Instances, TKey>> order, 
            int currentPage, 
            int itemsPerPage)
        {
            var Data = await Entities.Instances
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            int Count = await Entities.Instances
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
