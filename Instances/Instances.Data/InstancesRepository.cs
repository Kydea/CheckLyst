using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Data.Models;

namespace CheckListManager.Instances.Data
{
    public class InstancesRepository : Repository, IInstancesRepository
    {
        public InstancesRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        private const string INSTANCES_BASE_QUERY =
            "from " +
                "\"CheckLists\".\"Instances\" i " +
                "inner join \"CheckLists\".\"Prototypes\" p on i.\"Prototype\" = p.\"ID\" " +
                "inner join \"CheckLists\".\"PrototypeVersions\" pv on p.\"ID\" = pv.\"Prototype\" and i.\"Version\" = pv.\"Version\" " +
                "inner join \"Infrastructure\".\"Departments\" d on p.\"Department\" = d.\"ID\" " +
                "inner join \"CheckLists\".\"PrototypeTypes\" pt on p.\"PrototypeType\" = pt.\"ID\" " +
            "where " +
                "i.\"Company\" = :company " +
                "and (:id = -1 or i.\"ID\" = :id) " +
                "and (:description = '' or upper(i.\"Description\") like '%' || :description || '%') " +
                "and (:department = -1 or p.\"Department\" = :department) " +
                "and (:prototypetype = -1 or p.\"PrototypeType\" = :prototypetype) " +
                "and (:prototype = -1 or p.\"ID\" = :prototype) " +
                "and (:version = -1 or pv.\"Version\" = :version) " +
                "and (:year = -1 or i.\"Year\" = :year) " +
                "and (:number = -1 or i.\"Number\" = :number) " +
                "and (:from = '' or to_char(i.\"ModifiedOn\", 'yyyy-mm-dd') >= :from) " +
                "and (:to = '' or to_char(i.\"ModifiedOn\", 'yyyy-mm-dd') <= :to) " +
                "and (:tagValue = '' or exists(select 1 from \"CheckLists\".\"InstanceTags\" it where it.\"Instance\" = i.\"ID\" and (:tagCode = '' or it.\"Tag\" = :tagCode) and upper(it.\"Value\") like '%' || :tagValue || '%')) " +
                "and (:lifeCycleFrom = -1 or pv.\"LifeCycle\" > 0 and round(100 * extract(day from now() - i.\"CreatedOn\") / pv.\"LifeCycle\") >= :lifeCycleFrom) " +
                "and (:lifeCycleTo = -1 or pv.\"LifeCycle\" > 0 and round(100 * extract(day from now() - i.\"CreatedOn\") / pv.\"LifeCycle\") < :lifeCycleTo) " +
                "and (:lifeCycleFrom = -1 and :lifeCycleTo = -1 or pv.\"VersionState\" <> 'Draft' and pv.\"VersionState\" <> 'In progress' and pv.\"LifeCycle\" > 0 and i.\"Year\" <> 0 and i.\"InstanceState\" in ('In progress', 'Completed')) " +
                "and (" +
                    ":exceptionState = '' " +
                    "or :exceptionState = 'include' and exists(select 1 from \"CheckLists\".\"InstanceExpressionValues\" iev where iev.\"Instance\" = i.\"ID\" and iev.\"Exception\") " +
                    "or :exceptionState = 'exclude' and not exists(select 1 from \"CheckLists\".\"InstanceExpressionValues\" iev where iev.\"Instance\" = i.\"ID\" and iev.\"Exception\")" +
                ") " +
                "and (" +
                    ":exceptionDescription = '' " +
                    "or exists(select 1 from \"CheckLists\".\"InstanceExpressionValues\" iev where iev.\"Instance\" = i.\"ID\" and iev.\"Exception\" and upper(iev.\"Description\") like '%' || :exceptionDescription || '%')" +
                ") " +
                "and (" +
                    "i.\"Year\" = 0 and exists (" +
                        "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, 'Prototypes', 'Preview') g " +
                        "where g.\"Department\" = p.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = p.\"PrototypeType\")" +
                    ") " +
                    "or i.\"Year\" > 0 and exists (" +
                        "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, 'Instances', 'View') g " +
                        "where g.\"Department\" = p.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = p.\"PrototypeType\")" +
                    ")" +
                ") " +
                "and (" +
                    ":state = '' " +
                    "or :state <> 'Own' and i.\"InstanceState\" = :state " +
                    "or " +
                        ":state = 'Own' " +
                        "and exists (select 1 from \"Security\".\"GetGrantedUsers\" (p.\"Company\", p.\"Department\", p.\"PrototypeType\", 'Instances', 'Confirm') u where u.\"Code\" = :user) " +
                        "and exists (" +
                            "select 1 " +
                            "from " +
                                "\"WorkFlows\".\"WorkFlowApplications\" wfa " +
                                "inner join \"WorkFlows\".\"WorkFlowApplicationSteps\" wfas on wfa.\"ID\" = wfas.\"WorkFlowApplication\" " +
                                "inner join \"WorkFlows\".\"WorkFlowApplicationStepTags\" wfast on wfas.\"ID\" = wfast.\"WorkFlowApplicationStep\" " +
                            "where " +
                                "wfa.\"Instance\" = i.\"ID\" " +
                                "and wfas.\"User\" is null " +
                                "and not exists (select 1 from \"WorkFlows\".\"WorkFlowApplicationSteps\" ps where wfas.\"WorkFlowApplication\" = ps.\"WorkFlowApplication\" and ps.\"User\" is null and wfas.\"Step\" > ps.\"Step\") " +
                                "and not exists (select 1 from \"WorkFlows\".\"WorkFlowApplicationSteps\" ps where wfas.\"WorkFlowApplication\" = ps.\"WorkFlowApplication\" and ps.\"User\" = :user and wfas.\"Step\" = ps.\"Step\") " +
                                "and (" +
                                    "wfast.\"Tag\" is null " +
                                    "or exists (select 1 from \"Security\".\"UserLevels\" ul where ul.\"Company\" = p.\"Company\" and ul.\"User\" = :user and ul.\"Department\" = p.\"Department\" and (ul.\"PrototypeType\" is null or ul.\"PrototypeType\" = p.\"PrototypeType\") and ul.\"PrototypeWorkFlowLevel\" = wfast.\"Tag\") " +
                                ") " +
                    ")" +
                ")";

        private const string INSTANCES_LIST_QUERY =
            "select " +
                "i.\"ID\", i.\"Code\", " +
                "case when i.\"Year\" > 0 then cast(i.\"Year\" as text) when pv.\"VersionState\" = 'Draft' then 'Test' else 'SE' end \"Year\", " +
                "i.\"Number\", i.\"InstanceState\" \"State\", i.\"Description\", d.\"Name\" \"DepartmentName\", pt.\"Name\" \"PrototypeTypeName\", p.\"Name\" \"PrototypeName\", i.\"Version\", " +
                "case when pv.\"LifeCycle\" = 0 then null else extract(day from now() - i.\"CreatedOn\") || ' of ' || pv.\"LifeCycle\" || ' (' || round(100 * extract(day from now() - i.\"CreatedOn\") / pv.\"LifeCycle\") || '%)' end \"LifeCycle\", " +
                "i.\"CreatedOn\", i.\"CreatedBy\", i.\"ModifiedOn\", i.\"ModifiedBy\" " +
            INSTANCES_BASE_QUERY +
            "order by i.\"ID\" desc " +
            "limit :limit offset :offset";

        private const string INSTANCES_COUNT_QUERY = "select count(*) " + INSTANCES_BASE_QUERY;

        private const string OVERVIEW_QUERY =
            "select " +
                "i.\"ID\", i.\"Code\", i.\"Year\", i.\"Number\", i.\"InstanceState\" \"State\", p.\"Department\", d.\"Name\" \"DepartmentName\", p.\"PrototypeType\", pt.\"Name\" \"PrototypeTypeName\", p.\"Name\" \"PrototypeName\", i.\"Version\" " +
            INSTANCES_BASE_QUERY +
                "and pv.\"VersionState\" <> 'Draft' " +
                "and pv.\"VersionState\" <> 'In progress' " +
                "and i.\"Year\" <> 0 " +
            "order by d.\"Name\", pt.\"Name\", p.\"Name\", i.\"Version\", i.\"Year\", i.\"Number\" ";

        private const string LIFE_CYCLE_QUERY =
            "select " +
                "i.\"ID\", i.\"Code\", i.\"Year\", i.\"Number\", i.\"InstanceState\" \"State\", p.\"Department\", d.\"Name\" \"DepartmentName\", p.\"PrototypeType\", pt.\"Name\" \"PrototypeTypeName\", i.\"Prototype\", p.\"Name\" \"PrototypeName\", i.\"Version\", " +
                "extract(day from now() - i.\"CreatedOn\") \"Days\", pv.\"LifeCycle\" " +
            INSTANCES_BASE_QUERY +
            "order by d.\"Name\", pt.\"Name\", p.\"Name\", i.\"Version\", i.\"Year\", i.\"Number\" ";

        private const string TASKS_LIST_QUERY =
            "with i as (" +
                "select " +
                    "i.\"ID\", i.\"Code\", i.\"Year\", i.\"Number\", i.\"InstanceState\" \"State\", p.\"Department\", d.\"Name\" \"DepartmentName\", p.\"PrototypeType\", pt.\"Name\" \"PrototypeTypeName\", i.\"Prototype\", p.\"Name\" \"PrototypeName\", i.\"Version\", " +
                    "i.\"Description\" " +
                INSTANCES_BASE_QUERY +
                "and i.\"InstanceState\" = 'In progress'" +
            ") " +
            "select i.*, t.\"Description\" \"TaskDescription\", t.\"Code\" \"TaskCode\", t.\"Value\" \"TaskValue\" " +
            "from " +
                "i " +
                "inner join \"CheckLists\".\"InstanceExpressionValues\" t on i.\"ID\" = t.\"Instance\" and t.\"Task\" " +
            "order by i.\"DepartmentName\", i.\"PrototypeTypeName\", i.\"PrototypeName\", i.\"Version\", i.\"Year\", i.\"Number\", t.\"Description\", t.\"Code\"";

        private const string TASKS_COUNT_QUERY =
            "with i as (" +
                "select " +
                    "i.\"ID\" " +
                INSTANCES_BASE_QUERY +
                "and i.\"InstanceState\" = 'In progress'" +
            ") " +
            "select count(*) " +
            "from " +
                "i " +
                "inner join \"CheckLists\".\"InstanceExpressionValues\" t on i.\"ID\" = t.\"Instance\" and t.\"Task\"";

        private const string EXPORT_QUERY =
            "select " +
                "i.\"ID\", " +
                "case when i.\"Year\" > 0 then cast(i.\"Year\" as text) when pv.\"VersionState\" = 'Draft' then 'Test' else 'SE' end \"Year\", " +
                "i.\"Number\", i.\"InstanceState\" \"State\", i.\"Description\", p.\"Department\", d.\"Name\" \"DepartmentName\", p.\"PrototypeType\", pt.\"Name\" \"PrototypeTypeName\", i.\"Prototype\", p.\"Name\" \"PrototypeName\", i.\"Version\", " +
                "extract(day from now() - i.\"CreatedOn\") \"LifeDays\", pv.\"LifeCycle\" \"PrototypeLifeCycle\", " +
                "case when pv.\"LifeCycle\" = 0 then null else round(100 * extract(day from now() - i.\"CreatedOn\") / pv.\"LifeCycle\") || '%' end \"LifeCycle\", " +
                "i.\"CreatedOn\", i.\"CreatedBy\", i.\"ModifiedOn\", i.\"ModifiedBy\" " +
            INSTANCES_BASE_QUERY +
            "order by i.\"ID\" ";

        public async Task<PagingResponse<List<InstanceRecord>>> GetInstancesAsync
        (
            IDefaultIdentity identity,
            InstanceFilters filters,
            int currentPage,
            int itemsPerPage
        )
        {
            var Filters = new
            {
                company = identity.CompanyId,
                user = identity.Code,
                id = filters.ID ?? -1,
                description = filters.Description?.ToUpper() ?? string.Empty,
                department = filters.Department ?? -1,
                prototypetype = filters.PrototypeType ?? -1,
                prototype = filters.Prototype ?? -1,
                version = filters.Version ?? -1,
                year = filters.Year ?? -1,
                number = filters.Number ?? -1,
                state = filters.State ?? string.Empty,
                from = filters.From?.ToString("yyyy-MM-dd") ?? string.Empty,
                to = filters.To?.AddDays(1).ToString("yyyy-MM-dd") ?? string.Empty,
                tagCode = filters.TagCode ?? string.Empty,
                tagValue = filters.TagValue?.ToUpper() ?? string.Empty,
                exceptionState = filters.ExceptionState ?? string.Empty,
                exceptionDescription = filters.ExceptionDescription?.ToUpper() ?? string.Empty,
                limit = itemsPerPage,
                offset = (currentPage - 1) * itemsPerPage,
                lifeCycleFrom = filters.LifeCycleFrom ?? -1,
                lifeCycleTo = filters.LifeCycleTo ?? -1
            };

            var Count = await Read<int>(INSTANCES_COUNT_QUERY, Filters);
            var List = await Read<InstanceRecord>(INSTANCES_LIST_QUERY, Filters);
            foreach (var Record in List)
                Record.Exceptions = await Entities.InstanceExpressionValues.Where(iev => iev.Instance == Record.Id && iev.Exception).Select(iev => new InstanceExceptionRecord { Code = iev.Code, Description = iev.Description }).ToListAsync();

            return new PagingResponse<List<InstanceRecord>>
            {
                Data = List.ToList(),
                Count = Count.Single(),
                CurrentPage = currentPage,
                ItemsPerPage = itemsPerPage
            };
        }

        public async Task<List<OverviewRecord>> OverviewAsync
        (
            IDefaultIdentity identity,
            InstanceFilters filters
        )
        {
            var Filters = new
            {
                company = identity.CompanyId,
                user = identity.Code,
                id = filters.ID ?? -1,
                description = filters.Description?.ToUpper() ?? string.Empty,
                department = filters.Department ?? -1,
                prototypetype = filters.PrototypeType ?? -1,
                prototype = filters.Prototype ?? -1,
                version = filters.Version ?? -1,
                year = filters.Year ?? -1,
                number = filters.Number ?? -1,
                state = filters.State ?? string.Empty,
                from = filters.From?.ToString("yyyy-MM-dd") ?? string.Empty,
                to = filters.To?.AddDays(1).ToString("yyyy-MM-dd") ?? string.Empty,
                tagCode = filters.TagCode ?? string.Empty,
                tagValue = filters.TagValue?.ToUpper() ?? string.Empty,
                exceptionState = filters.ExceptionState ?? string.Empty,
                exceptionDescription = filters.ExceptionDescription?.ToUpper() ?? string.Empty,
                lifeCycleFrom = filters.LifeCycleFrom ?? -1,
                lifeCycleTo = filters.LifeCycleTo ?? -1
            };

            var List = await Read<OverviewRecord>(OVERVIEW_QUERY, Filters);
            return List.ToList();
        }

        public async Task<List<LifeCycleRecord>> LifeCycleAsync
        (
            IDefaultIdentity identity,
            InstanceFilters filters
        )
        {
            var Filters = new
            {
                company = identity.CompanyId,
                user = identity.Code,
                id = filters.ID ?? -1,
                description = filters.Description?.ToUpper() ?? string.Empty,
                department = filters.Department ?? -1,
                prototypetype = filters.PrototypeType ?? -1,
                prototype = filters.Prototype ?? -1,
                version = filters.Version ?? -1,
                year = filters.Year ?? -1,
                number = filters.Number ?? -1,
                state = filters.State ?? string.Empty,
                from = filters.From?.ToString("yyyy-MM-dd") ?? string.Empty,
                to = filters.To?.AddDays(1).ToString("yyyy-MM-dd") ?? string.Empty,
                tagCode = filters.TagCode ?? string.Empty,
                tagValue = filters.TagValue?.ToUpper() ?? string.Empty,
                exceptionState = filters.ExceptionState ?? string.Empty,
                exceptionDescription = filters.ExceptionDescription?.ToUpper() ?? string.Empty,
                lifeCycleFrom = filters.LifeCycleFrom ?? 0, //always applies life cycle extra filters
                lifeCycleTo = filters.LifeCycleTo ?? -1
            };

            var List = await Read<LifeCycleRecord>(LIFE_CYCLE_QUERY, Filters);
            return List.ToList();
        }

        public async Task<List<TasksRecord>> GetTasksAsync
        (
            IDefaultIdentity identity,
            InstanceFilters filters
        )
        {
            var Filters = new
            {
                company = identity.CompanyId,
                user = identity.Code,
                id = filters.ID ?? -1,
                description = filters.Description?.ToUpper() ?? string.Empty,
                department = filters.Department ?? -1,
                prototypetype = filters.PrototypeType ?? -1,
                prototype = filters.Prototype ?? -1,
                version = filters.Version ?? -1,
                year = filters.Year ?? -1,
                number = filters.Number ?? -1,
                state = filters.State ?? string.Empty,
                from = filters.From?.ToString("yyyy-MM-dd") ?? string.Empty,
                to = filters.To?.AddDays(1).ToString("yyyy-MM-dd") ?? string.Empty,
                tagCode = filters.TagCode ?? string.Empty,
                tagValue = filters.TagValue?.ToUpper() ?? string.Empty,
                exceptionState = filters.ExceptionState ?? string.Empty,
                exceptionDescription = filters.ExceptionDescription?.ToUpper() ?? string.Empty,
                lifeCycleFrom = filters.LifeCycleFrom ?? -1,
                lifeCycleTo = filters.LifeCycleTo ?? -1
            };

            var List = await Read<TasksRecord>(TASKS_LIST_QUERY, Filters);
            return List.ToList();
        }

        public async Task<long> CountTasksAsync (IDefaultIdentity identity)
        {
            var Filters = new
            {
                company = identity.CompanyId,
                user = identity.Code,
                id = -1,
                description = string.Empty,
                department = -1,
                prototypetype = -1,
                prototype = -1,
                version = -1,
                year = -1,
                number = -1,
                state = string.Empty,
                from = string.Empty,
                to = string.Empty,
                tagCode = string.Empty,
                tagValue = string.Empty,
                exceptionState = string.Empty,
                exceptionDescription = string.Empty,
                lifeCycleFrom = -1,
                lifeCycleTo = -1
            };

            var count = await ReadScalar<long>(TASKS_COUNT_QUERY, Filters);
            return count;
        }

        public async Task<List<InstanceRecord>> ExportAsync
        (
            InstanceFilters filters
        )
        {
            var Filters = new
            {
                company = filters.CompanyId,
                user = filters.User,
                id = filters.ID ?? -1,
                description = filters.Description?.ToUpper() ?? string.Empty,
                department = filters.Department ?? -1,
                prototypetype = filters.PrototypeType ?? -1,
                prototype = filters.Prototype ?? -1,
                version = filters.Version ?? -1,
                year = filters.Year ?? -1,
                number = filters.Number ?? -1,
                state = filters.State ?? string.Empty,
                from = filters.From?.ToString("yyyy-MM-dd") ?? string.Empty,
                to = filters.To?.AddDays(1).ToString("yyyy-MM-dd") ?? string.Empty,
                tagCode = filters.TagCode ?? string.Empty,
                tagValue = filters.TagValue?.ToUpper() ?? string.Empty,
                exceptionState = filters.ExceptionState ?? string.Empty,
                exceptionDescription = filters.ExceptionDescription?.ToUpper() ?? string.Empty,
                lifeCycleFrom = filters.LifeCycleFrom ?? -1,
                lifeCycleTo = filters.LifeCycleTo ?? -1
            };

            var List = await Read<InstanceRecord>(EXPORT_QUERY, Filters);
            return List.ToList();
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

        public async Task UpdateJSONFieldsAsync(long instance, long company, string itemsByCode, string itemsByLabel, string expressions) =>
            await Call<int>("\"CheckLists\".\"UpdateInstanceData\"",
                new Parameter("_Instance", instance),
                new Parameter("_Company", company),
                new Parameter("_ItemsByCode", itemsByCode),
                new Parameter("_ItemsByLabel", itemsByLabel),
                new Parameter("_Expressions", expressions)
            );

        public async Task<PrototypeVersions> GetPrototypeVersionAsync(long prototype, int version) =>
            await Entities.PrototypeVersions.SingleOrDefaultAsync(p => p.Prototype == prototype && p.Version == version);
    }
}
