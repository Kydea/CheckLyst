using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.Instances.Data
{
    public class InstancesRepository : Repository, IInstancesRepository
    {
        public InstancesRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public async Task<PagingResponse<List<TResult>>> GetInstancesAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Common.Data.Instances, TResult>> select,
            Expression<Func<Common.Data.Instances, bool>> filters,
            Expression<Func<Common.Data.Instances, TKey>> order, 
            int currentPage, 
            int itemsPerPage)
        {
            var data = await Entities.Instances
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            var count = await Entities.Instances
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

	    public async Task<Common.Data.Instances> CreateInstanceAsync(Common.Data.Instances instance)
	    {
		    try
		    {
			    if (instance == null)
			    {
				    throw new ArgumentNullException(nameof(instance), "Instance must be not null");
			    }
			    if (instance.Prototype == null)
			    {
				    throw new ArgumentNullException(nameof(instance.Prototype), "Prototype must be not null");
				}
			    if (instance.Version == null)
			    {
				    throw new ArgumentNullException(nameof(instance.Version), "Version must be not null");
			    }
			    if (instance.Year == null)
			    {
				    throw new ArgumentNullException(nameof(instance.Year), "Year must be not null");
			    }


				Entities.Database.BeginTransaction();

			    instance.Number =
				    await GetInstanceNumberAsync(instance.Prototype.Value, instance.Version.Value, instance.Year.Value);

				Entities.Instances.Add(instance);

			    await SaveChangesAsync();

			    Entities.Database.CurrentTransaction.Commit();

				return instance;
		    }
			finally
		    {
			    Entities.Database.CurrentTransaction?.Rollback();
		    }
	    }

	    public Task<int> GetInstanceNumberAsync(long prototype, int version, int year)
	    {
		    return Entities.Database.SqlQuery<int>("\"CheckLists\".\"GetInstanceNumber\"", prototype, version, year)
			    .FirstOrDefaultAsync();
	    }
    }
}
