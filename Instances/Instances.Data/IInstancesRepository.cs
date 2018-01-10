using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.Instances.Data
{
    public interface IInstancesRepository : IRepository
    {
        Task<PagingResponse<List<TResult>>> GetInstancesAsync<TResult, TKey>(long companyId, Expression<Func<Common.Data.Instances, TResult>> select, Expression<Func<Common.Data.Instances, bool>> filters, Expression<Func<Common.Data.Instances, TKey>> order = null, int? currentPage = null, int? itemsPerPage = null);
	    Task<Common.Data.Instances> CreateInstanceAsync(Common.Data.Instances instance);
	    Task<Common.Data.Instances> GetByCodeAsync(long companyId, string code);
	    Task<Common.Data.Instances> GetByIDAsync(long companyId, long id);
		Task<int> GetInstanceNumberAsync(long prototype, int version, int year);
	    Task<List<Common.Data.Instances>> GetInstancesWithExpressions(string state = null);
        void DeleteInstance(Common.Data.Instances instance);
        void SaveChildren(Common.Data.Instances instance, IEnumerable<InstanceTags> instanceTags, IEnumerable<InstanceExpressionValues> instanceExpressionValues);
    }
}