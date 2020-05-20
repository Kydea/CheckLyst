using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
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
				query = query.OrderByDescending(order);
			if (currentPage.HasValue && itemsPerPage.HasValue)
				query = query.Skip((currentPage.Value - 1) * itemsPerPage.Value).Take(itemsPerPage.Value);

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

		public async Task<Common.Data.Instances> BeginCreateInstanceAsync(Common.Data.Instances instance)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance), "Instance must be not null");

			Entities.Database.BeginTransaction();

			instance.Number =
				await GetInstanceNumberAsync(instance.Prototype, instance.Version, instance.Year);

            instance.InstancesHistory.Add(new InstancesHistory
            {
                Date = instance.CreatedOn,
                User = instance.CreatedBy,
                State = InstanceState.Draft
            });

            instance.InstancesHistory.Add(new InstancesHistory
            {
                Date = instance.ModifiedOn.Value,
                User = instance.ModifiedBy,
                State = instance.InstanceState
            });

			Entities.Instances.Add(instance);

			await SaveChangesAsync();

			await Entities.Entry(instance).ReloadAsync();

			return instance;
		}

        public void EndCreateInstanceAsync(Common.Data.Instances instance)
        {
            Entities.Database.CurrentTransaction.Commit();
        }

        public void CancelCreateInstanceAsync()
        {
            Entities.Database.CurrentTransaction?.Rollback();

        }

        public Task<Common.Data.Instances> GetAsync(long companyId, string code)
			=> Entities.Instances.SingleOrDefaultAsync(i => i.Prototypes.Company == companyId && i.Code == code);

        public Task<Common.Data.Instances> GetAsync(long companyId, long id)
            => Entities.Instances.SingleOrDefaultAsync(i => i.Prototypes.Company == companyId && i.ID == id);

        public Task<string> GetCodeAsync(long companyId, long id)
            => Entities.Instances.Where(i => i.Prototypes.Company == companyId && i.ID == id).Select(i => i.Code).SingleOrDefaultAsync();

        public Task<int> GetInstanceNumberAsync(long prototype, int version, int year)
		{
			return Call<int>("\"CheckLists\".\"GetInstanceNumber\"",
				new Parameter("_prototype", prototype),
				new Parameter("_version", version),
				new Parameter("_year", year)
			);
		}

		public Task<List<Common.Data.Instances>> GetInstancesWithExpressions(long Prototype, int Version)
		{
			return Entities.Instances.Include(i => i.InstanceExpressionValues).Where(i => i.Prototype == Prototype && i.Version == Version && i.InstanceState == InstanceState.Progress).ToListAsync();
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

        public async Task ValidateUniqueExpressions(long instance)
        {
            await Call<int>("\"CheckLists\".\"ValidateUniqueExpressions\"",
                new Parameter("_instance", instance)
            );
        }

        public async Task<int?> GetActivePrototypeVersion(long prototype)
        {
            return (await Entities.PrototypeVersions.SingleOrDefaultAsync(p => p.Prototype == prototype && p.VersionState == VersionState.Active))?.Version;
        }

        public async Task<List<Users>> GetInstanceEditors(long department, long prototypeType, string versionState)
        {
            var Users =
                versionState == VersionState.Draft || versionState == VersionState.InSimulation ?
                    Entities.UserFunctions.Where(uf =>
                        uf.Department == department && (
                            uf.PrototypeType == null 
                            || uf.PrototypeType == prototypeType
                        )
                        && uf.FunctionGroup == "Prototypes" && uf.Function == "Edit").Select(uf => uf.Users).Distinct().ToListAsync()
                    : Entities.UserFunctions.Where(uf =>
                        uf.Department == department && (
                            uf.PrototypeType == null
                            || uf.PrototypeType == prototypeType
                        )
                        && uf.FunctionGroup == "Instances" && uf.Function == "Edit").Select(uf => uf.Users).Distinct().ToListAsync();

            return await Users;
        }

        public async Task<List<InstanceExpressionValues>> GetNotifications() =>
            await 
                Entities.InstanceExpressionValues
                    .Include(i => i.Instances)
                    .Include(i => i.Instances.PrototypeVersions)
                    .Where(n => n.Notification && !n.Notified && n.NotificationDate <= DateTimeOffset.UtcNow)
                    .ToListAsync();
    }
}
