using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Prototypes.Data
{
    public class PrototypesRepository : Repository, IPrototypesRepository
    {
        public PrototypesRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public async Task<PagingResponse<List<TResult>>> GetPrototypesAsync<TResult, TKey>(
            long companyId,
            Expression<Func<PrototypeVersions, TResult>> select,
            Expression<Func<PrototypeVersions, bool>> filters,
            Expression<Func<PrototypeVersions, TKey>> order, 
            int currentPage, 
            int itemsPerPage)
        {
            var data = await Entities.PrototypeVersions
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            var count = await Entities.PrototypeVersions
                .Where(filters)
                .CountAsync();

            return new PagingResponse<List<TResult>>
            {
                Data = data,
                Count = count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }

        public async Task<Common.Data.Prototypes> GetPrototypeAsync(DefaultIdentity defaultIdentity, long id)
        {
            var result = await Entities.Prototypes.SingleOrDefaultAsync(x => x.Company == defaultIdentity.CompanyId && x.ID == id);
            if (result == null)
                throw new Exception($"Prototype {id} not found");
            return result;
        }

        public async Task<PrototypeVersions> GetPrototypeVersionAsync(DefaultIdentity defaultIdentity, long prototype, int version)
        {
            var result = await Entities.PrototypeVersions.SingleOrDefaultAsync(x => x.Prototypes.Company == defaultIdentity.CompanyId && x.Prototype == prototype && x.Version == version);
            if (result == null)
                throw new Exception($"Prototype version {prototype}.{version} not found");
            return result;
        }

        public void AddPrototype(Common.Data.Prototypes prototype)
        {
            Entities.Prototypes.Add(prototype);
        }

        public void AddPrototypeVersion(PrototypeVersions prototypeVersion)
        {
            Entities.PrototypeVersions.Add(prototypeVersion);
        }

        public async Task SaveAsync()
        {
            await SaveChangesAsync();
        }
    }
}
