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
	    Task<Common.Data.Instances> BeginCreateInstanceAsync(Common.Data.Instances instance);
	    void EndCreateInstanceAsync(Common.Data.Instances instance);
	    void CancelCreateInstanceAsync();
	    Task<Common.Data.Instances> GetAsync(long companyId, string code);
	    Task<Common.Data.Instances> GetAsync(long companyId, long id);
        Task<string> GetCodeAsync(long company, long iD);
        Task<int> GetInstanceNumberAsync(long prototype, int version, int year);
	    Task<List<Common.Data.Instances>> GetInstancesWithExpressions(long Prototype, int Version);
        void DeleteInstance(Common.Data.Instances instance);
        void SaveChildren(long instanceID, IEnumerable<InstanceTags> instanceTags, IEnumerable<InstanceExpressionValues> instanceExpressionValues);
        void SaveTags(long instanceID, IEnumerable<InstanceTags> instanceTags);
        void SaveExpressionValues(long instanceID, IEnumerable<InstanceExpressionValues> instanceExpressionValues);
        Task ValidateUniqueExpressions(long instance);
        Task<int?> GetActivePrototypeVersion(long prototype);
        Task<List<Users>> GetInstanceEditors(long department, long prototypeType, string versionState);
        Task<List<InstanceExpressionValues>> GetNotifications();
    }
}