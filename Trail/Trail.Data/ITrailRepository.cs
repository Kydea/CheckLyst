using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq.Expressions;

namespace CheckListManager.Trail.Data
{
	public interface ITrailRepository : IRepository
	{
		Task AddAsync(Records record);

        Task<List<TResult>> GetRecordsAsync<TResult, TKey>(long companyId,
            Expression<Func<Records, TResult>> select,
            Expression<Func<Records, bool>> filters,
            Expression<Func<Records, TKey>> order);

        Task<PagingResponse<List<TResult>>> GetRecordsAsync<TResult, TKey>(long companyId,
            Expression<Func<Records, TResult>> select,
            Expression<Func<Records, bool>> filters,
            Expression<Func<Records, TKey>> order,
            int currentPage,
            int itemsPerPage);
    }
}
