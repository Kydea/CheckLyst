using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.DataSources.Data
{
    public class DataSourcesRepository : Repository, IDataSourcesRepository
    {
        public DataSourcesRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public async Task<PagingResponse<List<TResult>>> GetDataSourcesAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Common.Data.DataSources, TResult>> select,
            Expression<Func<Common.Data.DataSources, bool>> filters,
            Expression<Func<Common.Data.DataSources, TKey>> order, 
            int currentPage, 
            int itemsPerPage)
        {
            var Data = await Entities.DataSources
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            int Count = await Entities.DataSources
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
