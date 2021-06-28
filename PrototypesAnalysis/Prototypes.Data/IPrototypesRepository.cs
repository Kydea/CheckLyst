using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Prototypes.Data.Models;
using System.Linq;

namespace CheckListManager.Prototypes.Data
{
    public interface IPrototypesRepository : IRepository
    {
        Task<List<TResult>> GetPrototypesAsync<TResult, TKey>(long companyId,
            Expression<Func<Common.Data.Prototypes, TResult>> select, 
            Expression<Func<Common.Data.Prototypes, bool>> filters,
            Expression<Func<Common.Data.Prototypes, TKey>> order);

        Task<List<SyntheticRecord>> GetPrototypesAsync(IDefaultIdentity identity);

        Task<PagingResponse<List<PrototypeRecord>>> GetPrototypeVersionsAsync(
            IDefaultIdentity identity,
            PrototypeFilters filters,
            int currentPage,
            int itemsPerPage);

        Task<Common.Data.Prototypes> GetPrototypeAsync(DefaultIdentity defaultIdentity, long id);
        Task<PrototypeVersions> GetPrototypeVersionAsync(long companyId, long prototype, int version);
        Task CheckNewVersionAsync(long companyId, long prototype);
        void AddPrototype(Common.Data.Prototypes prototype);
        void AddPrototypeVersion(PrototypeVersions prototypeVersion);
        Task DeletePrototypeVersionAsync(DefaultIdentity defaultIdentity, long prototype, int version);
        void DeletePrototypeVersionTags(IEnumerable<PrototypeVersionTags> versionTags);
        
        Task<SetStateResult> SetStateAsync(DefaultIdentity defaultIdentity, PrototypeVersions prototypeVersion, string state,
            bool proposed);

        Task<List<PrototypeVersions>> GetPrototypeVersionsAsync(Expression<Func<PrototypeVersions, bool>> filters);
        IQueryable<PrototypeVersions> GetPrototypeVersions();
    }
}