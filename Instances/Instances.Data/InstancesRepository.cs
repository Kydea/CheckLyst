using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Data.ListItems;

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
                .Where(i => i.Prototypes.Company == companyId)
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
            => Entities.Instances.Include(i => i.PrototypeVersions).SingleOrDefaultAsync(i => i.Prototypes.Company == companyId && i.ID == id);

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

        public void SaveChildren(long instanceID, IEnumerable<InstanceTags> instanceTags, IEnumerable<InstanceExpressionValues> instanceExpressionValues)
        {
            SaveTags(instanceID, instanceTags);
            SaveExpressionValues(instanceID, instanceExpressionValues);
        }

        public void SaveTags(long instanceID, IEnumerable<InstanceTags> instanceTags)
        {
            Entities.InstanceTags.RemoveRange(Entities.InstanceTags.Where(x => x.Instance == instanceID));
            Entities.InstanceTags.AddRange(instanceTags);
        }

        public void SaveExpressionValues(long instanceID, IEnumerable<InstanceExpressionValues> instanceExpressionValues)
        {
            //Notified flag synchronization
            var OldExpressionValues = Entities.InstanceExpressionValues.Where(x => x.Instance == instanceID);
            var OldNotifications = OldExpressionValues.Where(e => e.Notification).ToDictionary(k => k.Code, v => v);
            instanceExpressionValues.Where(iv => iv.Notification).ToList().ForEach(iv =>
            {
                OldNotifications.TryGetValue(iv.Code, out var ov);
                iv.Notified =
                    iv.Notified //the value has not changed since last save
                    && ov != null && ov.Notified //last notification has not been sent yet
                ;
            });

            Entities.InstanceExpressionValues.RemoveRange(OldExpressionValues);
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

        public async Task<List<Users>> GetInstanceEditors(long company, long department, long prototypeType, string versionState)
        {
            var Filters = new
            {
                company,
                department,
                prototypetype = prototypeType, 
                functiongroup = versionState == VersionState.Draft || versionState == VersionState.InSimulation ? "Prototypes" : "Instances",
                function = "Edit"
            };

            var List = await Read<Users>("select * from \"Security\".\"GetGrantedUsers\"(:company, :department, :prototypetype, :functiongroup, :function) u order by u.\"Code\"", Filters);
            return List.ToList();
        }

        public async Task<List<InstanceExpressionValues>> GetNotifications() =>
            await
                Entities.InstanceExpressionValues
                    .Include(i => i.Instances)
                    .Include(i => i.Instances.PrototypeVersions)
                    .Where(n => n.Notification && !n.Notified && n.NotificationDate <= DateTimeOffset.UtcNow)
                    .ToListAsync();

        public async Task<List<InstanceRelation>> GetParentInstances(long Instance) =>
            await
                Entities.InstanceRelations.Where(r => r.Child == Instance).Select(r => new InstanceRelation
                {
                    Item = r.Item,
                    ID = r.ParentInstance.ID,
                    Code = r.ParentInstance.Code,
                    Prototype = r.ParentInstance.PrototypeName,
                    Version = r.ParentInstance.Version,
                    Year = r.ParentInstance.Year,
                    Number = r.ParentInstance.Number,
                    Description = r.ParentInstance.Description,
                    VersionState = r.ParentInstance.PrototypeVersions.VersionState,
                    RelationType = InstanceRelation.RelationTypes.Parent
                })
                .OrderBy(r => r.ID).ThenBy(r => r.Item)
                .ToListAsync();

        public async Task DeleteRelation(long parent, int item)
        {
            var Relation = await Entities.InstanceRelations.SingleOrDefaultAsync(r => r.Parent == parent && r.Item == item);
            if (Relation != null) Entities.InstanceRelations.Remove(Relation);
        }

        public void AddRelation(long parent, int item, long child) =>
            Entities.InstanceRelations.Add(new InstanceRelations { Parent = parent, Item = item, Child = child });
    }
}
