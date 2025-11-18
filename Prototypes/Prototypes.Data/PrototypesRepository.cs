using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.WorkFlows;
using CheckListManager.Prototypes.Data.Models;

namespace CheckListManager.Prototypes.Data
{
    public class PrototypesRepository : Repository, IPrototypesRepository
    {
        private readonly IWorkFlowsService _workFlowsService;

        public PrototypesRepository(RelationalDatabaseEntities entities, IWorkFlowsService workFlowsService) : base(entities)
        {
            _workFlowsService = workFlowsService;
        }

        public async Task<List<TResult>> GetPrototypesAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Common.Data.Prototypes, TResult>> select,
            Expression<Func<Common.Data.Prototypes, bool>> filters,
            Expression<Func<Common.Data.Prototypes, TKey>> order
        )
        {
            var data = await Entities.Prototypes
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .ToListAsync();
            return data;
        }

        private const string PROTOTYPES_QUERY =
            "select p.\"ID\", p.\"Name\" " +
            "from \"CheckLists\".\"Prototypes\" p " +
            "where " +
                "p.\"Company\" = :company " +
                "and exists (" +
                    "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, 'Prototypes', 'View') g " +
                    "where g.\"Department\" = p.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = p.\"PrototypeType\")" +
                ") " +
            "order by p.\"Name\"";
        public async Task<List<SyntheticRecord>> GetPrototypesAsync(IDefaultIdentity identity)
        {
            var Filters = new
            {
                company = identity.CompanyId,
                user = identity.Code
            };

            var List = await Read<SyntheticRecord>(PROTOTYPES_QUERY, Filters);
            return List.ToList();
        }

        private const string PROTOTYPE_VERSIONS_BASE_QUERY =
            "from " +
                "\"CheckLists\".\"Prototypes\" p " +
                "inner join \"CheckLists\".\"PrototypeVersions\" pv on p.\"ID\" = pv.\"Prototype\" " +
                "inner join \"Infrastructure\".\"Departments\" d on p.\"Department\" = d.\"ID\" " +
                "inner join \"CheckLists\".\"PrototypeTypes\" pt on p.\"PrototypeType\" = pt.\"ID\" " +
            "where " +
                "p.\"Company\" = :company " +
                "and (:prototype = -1 or p.\"ID\" = :prototype) " +
                "and (:department = -1 or p.\"Department\" = :department) " +
                "and (:prototypetype = -1 or p.\"PrototypeType\" = :prototypetype) " +
                "and (:state = '' or pv.\"VersionState\" = :state) " +
                "and (:name = '' or upper(p.\"Name\") like '%' || :name || '%') " +
                "and (:pending = 'Include' or :pending = 'Exclude' and pv.\"ProposedState\" is null or :pending = 'Filter' and not pv.\"ProposedState\" is null) " +
                "and exists (" +
                    "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, 'Prototypes', 'View') g " +
                    "where g.\"Department\" = p.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = p.\"PrototypeType\")" +
                ") ";
        private const string PROTOTYPE_VERSIONS_LIST_QUERY = 
            "select " +
                "p.\"ID\", p.\"Name\", pv.\"Description\", pv.\"VersionState\" \"State\", pv.\"Version\", pv.\"ValidFrom\" \"ValidityFrom\", pv.\"ValidUntil\" \"ValidityUntil\", " +
                "p.\"Department\", d.\"Name\" \"DepartmentName\", p.\"PrototypeType\", pt.\"Name\" \"PrototypeTypeName\", pv.\"LifeCycle\", " +
                "pv.\"CreatedOn\", pv.\"CreatedBy\", pv.\"ModifiedOn\", pv.\"ModifiedBy\" " +
            PROTOTYPE_VERSIONS_BASE_QUERY + 
            "order by p.\"ID\", pv.\"Version\" " +
            "limit :limit offset :offset";
        private const string PROTOTYPE_VERSIONS_COUNT_QUERY = "select count(*) " + PROTOTYPE_VERSIONS_BASE_QUERY;

        public async Task<PagingResponse<List<PrototypeRecord>>> GetPrototypeVersionsAsync(
            IDefaultIdentity identity,
            PrototypeFilters PrototypeFilters,
            int currentPage,
            int itemsPerPage)
        {
            var Filters = new
            {
                company = identity.CompanyId,
                user = identity.Code,
                prototype = PrototypeFilters.ID ?? -1,
                department = PrototypeFilters.Department ?? -1,
                prototypetype = PrototypeFilters.PrototypeType ?? -1,
                state = PrototypeFilters.State ?? string.Empty,
                name = PrototypeFilters.Name?.ToUpper() ?? string.Empty,
                pending = PrototypeFilters.Pending,
                limit = itemsPerPage,
                offset = (currentPage - 1) * itemsPerPage
            };

            var List = await Read<PrototypeRecord>(PROTOTYPE_VERSIONS_LIST_QUERY, Filters);
            var Count = await Read<int>(PROTOTYPE_VERSIONS_COUNT_QUERY, Filters);
            return new PagingResponse<List<PrototypeRecord>>
            {
                Data = List.ToList(),
                Count = Count.Single(),
                CurrentPage = currentPage,
                ItemsPerPage = itemsPerPage
            };
        }

        public async Task<Common.Data.Prototypes> GetPrototypeAsync(DefaultIdentity defaultIdentity, long id)
        {
            var result = await Entities.Prototypes.SingleOrDefaultAsync(x => x.Company == defaultIdentity.CompanyId && x.ID == id) ?? throw new Exception($"Prototype {id} not found");
            return result;
        }

        public async Task<PrototypeVersions> GetPrototypeVersionAsync(long companyId, long prototype, int version)
        {
            var result = await Entities.PrototypeVersions.SingleOrDefaultAsync(x => x.Prototypes.Company == companyId && x.Prototype == prototype && x.Version == version) ?? throw new Exception($"Prototype version {prototype}.{version} not found");
            return result;
        }

        public async Task CheckNewVersionAsync(long companyId, long prototype)
        {
            if (await Entities.PrototypeVersions.AnyAsync(x => x.Prototypes.Company == companyId && x.Prototype == prototype && (x.VersionState == VersionState.Draft || x.VersionState == VersionState.InSimulation || x.VersionState == VersionState.InApproval)))
                throw new Exception("There is already a new version under processing");
        }

        public void AddPrototype(Common.Data.Prototypes prototype)
        {
            Entities.Prototypes.Add(prototype);
        }

        public void AddPrototypeVersion(PrototypeVersions prototypeVersion)
        {
            Entities.PrototypeVersions.Add(prototypeVersion);
        }

        public async Task DeletePrototypeVersionAsync(DefaultIdentity defaultIdentity, long prototype, int version)
        {
            var PrototypeVersion = await Entities.PrototypeVersions.SingleOrDefaultAsync(x => x.Prototypes.Company == defaultIdentity.CompanyId && x.Prototype == prototype && x.Version == version) ?? throw new Exception($"Prototype version {prototype}.{version} not found");
            if (PrototypeVersion.VersionState != VersionState.Draft)
                throw new Exception("The prototype version is not in state draft");

            var Prototype = PrototypeVersion.Prototypes;
            Entities.PrototypeVersions.Remove(PrototypeVersion);
            if (!Prototype.PrototypeVersions.Any())
                Entities.Prototypes.Remove(Prototype);
            await SaveChangesAsync();
        }

        public void DeletePrototypeVersionTags(IEnumerable<PrototypeVersionTags> versionTags)
        {
            Entities.PrototypeVersionTags.RemoveRange(versionTags);
        }

        public async Task<SetStateResult> SetStateAsync(DefaultIdentity defaultIdentity, PrototypeVersions prototypeVersion, string state, bool proposed)
        {
            var result = new SetStateResult();

            if (prototypeVersion.VersionState == VersionState.Draft && state == VersionState.InSimulation && !proposed)
            {
                prototypeVersion.VersionState = state;
                prototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
                {
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Date = DateTimeOffset.UtcNow,
                    State = state,
                    User = defaultIdentity.Code
                });
                Entities.Instances.RemoveRange(prototypeVersion.Instances);
                result.HasInstancesDeleted = true;
                result.HasToInvalidateCache = true;
            }
            else if (prototypeVersion.VersionState == VersionState.InSimulation && state == VersionState.Draft && !proposed)
            {
                prototypeVersion.VersionState = state;
                prototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
                {
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Date = DateTimeOffset.UtcNow,
                    State = state,
                    User = defaultIdentity.Code
                });
                Entities.Instances.RemoveRange(prototypeVersion.Instances);
                result.HasInstancesDeleted = true;
                result.HasToInvalidateCache = true;
            }
            else if (prototypeVersion.VersionState == VersionState.InSimulation && state == VersionState.Approved && proposed)
            {
                var WorkFlow = prototypeVersion.Prototypes.PrototypeTypes.ApprovationWorkFlow ?? throw new InvalidOperationException($"Approval workflow not found for prototype type {prototypeVersion.Prototypes.PrototypeTypes.Name} ({prototypeVersion.Prototypes.PrototypeType}).");

                var response = await _workFlowsService.CreateWorkFlowApplicationAsync(defaultIdentity, new CreateRequest
                {
                    WorkFlow = WorkFlow,
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Action = "Prototype – state to Approved/Active"
                });

                prototypeVersion.VersionState = VersionState.InApproval;
                prototypeVersion.ProposedState = state;
                prototypeVersion.ProposedDate = DateTimeOffset.UtcNow;

                prototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
                {
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Date = DateTimeOffset.UtcNow,
                    State = VersionState.InApproval,
                    User = defaultIdentity.Code
                });

                prototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
                {
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Date = DateTimeOffset.UtcNow,
                    State = state,
                    User = defaultIdentity.Code,
                    WorkFlowApplication = response.ID
                });

                result.HasToInvalidateCache = true;
            }
            else if ((prototypeVersion.VersionState == VersionState.Approved || prototypeVersion.VersionState == VersionState.Active) && state == VersionState.Obsolete && proposed)
            {
                var WorkFlow = prototypeVersion.Prototypes.PrototypeTypes.ObsolescenceWorkFlow ?? throw new InvalidOperationException($"Obsolescence workflow not found for prototype type {prototypeVersion.Prototypes.PrototypeTypes.Name} ({prototypeVersion.Prototypes.PrototypeType}).");

                var response = await _workFlowsService.CreateWorkFlowApplicationAsync(defaultIdentity, new CreateRequest
                {
                    WorkFlow = WorkFlow,
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Action = "Prototype – state to Obsolete"
                });

                prototypeVersion.ProposedState = state;
                prototypeVersion.ProposedDate = DateTimeOffset.UtcNow;

                prototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
                {
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Date = DateTimeOffset.UtcNow,
                    State = state,
                    User = defaultIdentity.Code,
                    WorkFlowApplication = response.ID
                });
            }
            else
                throw new InvalidOperationException($"Cannot {(proposed ? "propose" : "change")} state from {prototypeVersion.VersionState} to {state}");

            await SaveChangesAsync();

            return result;
        }

        public Task<List<PrototypeVersions>> GetPrototypeVersionsAsync(Expression<Func<PrototypeVersions, bool>> filters) =>
            Entities.PrototypeVersions.Where(filters).OrderBy(p => p.Prototype).ThenBy(p => p.Version).ToListAsync();

        public Task<long> CountPrototypesAsync() => 
            Entities.Prototypes.LongCountAsync();

    }
}
