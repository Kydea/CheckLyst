using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Data.Models;

namespace CheckListManager.Instances.Data
{
    public interface IInstancesRepository : IRepository
    {
        Task<PagingResponse<List<InstanceRecord>>> GetInstancesAsync(IDefaultIdentity identity, InstanceFilters filters, int currentPage, int itemsPerPage);
        Task<List<OverviewRecord>> OverviewAsync(IDefaultIdentity identity, InstanceFilters filters);
        Task<List<LifeCycleRecord>> LifeCycleAsync(IDefaultIdentity identity, InstanceFilters filters);
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
        Task<List<Users>> GetInstanceEditors(long company, long department, long prototypeType, string versionState);
        Task<List<InstanceExpressionValues>> GetNotifications();
        Task<List<InstanceRelation>> GetParentInstances(long Instance);
        Task DeleteRelation(long parent, int item);
        void AddRelation(long parent, int item, long child);
        Task UpdateJSONFieldsAsync(long instance, long company, string itemsByCode, string itemsByLabel, string expressions);
        Task<PrototypeVersions> GetPrototypeVersionAsync(long prototype, int version);
    }
}