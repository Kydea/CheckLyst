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
			Expression<Func<Common.Data.Instances, TKey>> order = null,
			int? currentPage = null,
			int? itemsPerPage = null)
		{
			var query = Entities.Instances
				.Where(filters)
				.Where(i => i.Prototypes.Company == companyId);

			if (order != null)
			{
				query = query.OrderBy(order);
			}
			if (currentPage.HasValue && itemsPerPage.HasValue)
			{
				query = query.Skip((currentPage.Value - 1) * itemsPerPage.Value).Take(itemsPerPage.Value);
			}

			var data = await
				query
				.Select(select)
				.ToListAsync();

			var count = await Entities.Instances
				.Where(filters)
				.CountAsync();

			return new PagingResponse<List<TResult>>
			{
				Data = data,
				Count = count,
				ItemsPerPage = itemsPerPage ?? 1,
				CurrentPage = currentPage ?? -1
			};
		}

		public async Task<Common.Data.Instances> CreateInstanceAsync(Common.Data.Instances instance)
		{
			try
			{
				if (instance == null)
					throw new ArgumentNullException(nameof(instance), "Instance must be not null");

				Entities.Database.BeginTransaction();

				instance.Number =
					await GetInstanceNumberAsync(instance.Prototype, instance.Version, instance.Year);

                instance.InstancesHistory.Add(new InstancesHistory
                {
                    Date = instance.ModifiedOn.Value,
                    User = instance.ModifiedBy,
                    State = instance.InstanceState
                });

				Entities.Instances.Add(instance);

				await SaveChangesAsync();

				Entities.Database.CurrentTransaction.Commit();

				await Entities.Entry(instance).ReloadAsync();

				return instance;
			}
			catch
			{
				Entities.Database.CurrentTransaction?.Rollback();
				throw;
			}
		}

		public Task<Common.Data.Instances> GetByCodeAsync(long companyId, string code)
			=> Entities.Instances.SingleOrDefaultAsync(i => i.Prototypes.Company == companyId && i.Code == code);

        public Task<Common.Data.Instances> GetByIDAsync(long companyId, long id)
            => Entities.Instances.SingleOrDefaultAsync(i => i.Prototypes.Company == companyId && i.ID == id);

        public Task<int> GetInstanceNumberAsync(long prototype, int version, int year)
		{
			return Call<int>("\"CheckLists\".\"GetInstanceNumber\"",
				new Parameter("_prototype", prototype),
				new Parameter("_version", version),
				new Parameter("_year", year)
			);
		}

		public Task<List<Common.Data.Instances>> GetInstancesWithExpressions(string state = null)
		{
			//Can't do state == null || i.InstanceState == null, not supported by Npgsql
			var query = Entities.Instances.Include(i => i.InstanceExpressionValues);
			if (state != null)
				query = query.Where(i => i.InstanceState == state);
			return query.ToListAsync();
		}

        public void DeleteInstance(Common.Data.Instances instance)
        {
            Entities.Instances.Remove(instance);
        }

        public void SaveChildren(Common.Data.Instances instance, IEnumerable<InstanceTags> instanceTags, IEnumerable<InstanceExpressionValues> instanceExpressionValues)
        {
            Entities.InstanceTags.RemoveRange(Entities.InstanceTags.Where(x => x.Instance == instance.ID));
            Entities.InstanceTags.AddRange(instanceTags);

            Entities.InstanceExpressionValues.RemoveRange(instance.InstanceExpressionValues);
            Entities.InstanceExpressionValues.AddRange(instanceExpressionValues);
        }

    }
}
