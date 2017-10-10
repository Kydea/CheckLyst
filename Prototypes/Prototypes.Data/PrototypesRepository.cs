using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;

namespace CheckListManager.Prototypes.Data
{
    public class PrototypesRepository : Repository, IPrototypesRepository
    {
        public PrototypesRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public Task<List<TResult>> GetPrototypesAsync<TResult, TKey>(
            long companyId,
            Expression<Func<PrototypeVersions, TResult>> select,
            Expression<Func<PrototypeVersions, bool>> filters, 
            Expression<Func<PrototypeVersions, TKey>> order, 
            int skip, 
            int take
        )
            => Entities.PrototypeVersions
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

    }
}
