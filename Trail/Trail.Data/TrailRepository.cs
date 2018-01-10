using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;

namespace CheckListManager.Trail.Data
{
	public class TrailRepository : Repository, ITrailRepository
	{
		public TrailRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public async Task AddAsync(Records record)
		{
			Entities.Records.Add(record);
			await SaveChangesAsync();
		}

        public async Task<List<TResult>> GetRecordsAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Records, TResult>> select,
            Expression<Func<Records, bool>> filters,
            Expression<Func<Records, TKey>> order
            )
        {
            return await Entities.Records
                .Where(filters)
                .OrderByDescending(order).ThenByDescending(r => r.ID)
                .Select(select)
                .ToListAsync();
        }

        public async Task<PagingResponse<List<TResult>>> GetRecordsAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Records, TResult>> select,
            Expression<Func<Records, bool>> filters,
            Expression<Func<Records, TKey>> order,
            int currentPage,
            int itemsPerPage)
        {
            var Data = await Entities.Records
                .Where(filters)
                .OrderByDescending(order).ThenByDescending(r => r.ID)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            int Count = await Entities.Records
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
