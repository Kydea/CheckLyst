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

namespace CheckListManager.Prototypes.Data
{
    public class PrototypesRepository : Repository, IPrototypesRepository
    {
        IWorkFlowsService _workFlowsService;

        public PrototypesRepository(RelationalDatabaseEntities entities, IWorkFlowsService workFlowsService) : base(entities)
		{
            _workFlowsService = workFlowsService;
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

        public async Task DeletePrototypeVersionAsync(DefaultIdentity defaultIdentity, long prototype, int version)
        {
            var PrototypeVersion = await Entities.PrototypeVersions.SingleOrDefaultAsync(x => x.Prototypes.Company == defaultIdentity.CompanyId && x.Prototype == prototype && x.Version == version);
            if (PrototypeVersion == null)
                throw new Exception($"Prototype version {prototype}.{version} not found");
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

        public async Task SetStateAsync(DefaultIdentity defaultIdentity, PrototypeVersions prototypeVersion, string state, bool proposed)
        {
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
            }
            else if (prototypeVersion.VersionState == VersionState.InSimulation && state == VersionState.Approved && proposed)
            {
                var WorkFlow = prototypeVersion.Prototypes.PrototypeTypes.ApprovationWorkFlow;
                if (WorkFlow == null)
                    throw new InvalidOperationException($"Approvation workflow not found for prototype type {prototypeVersion.Prototypes.PrototypeTypes.Name} ({prototypeVersion.Prototypes.PrototypeType}).");

                var response = await _workFlowsService.CreateWorkFlowApplicationAsync(defaultIdentity, new CreateRequest
                {
                    WorkFlow = WorkFlow.Value,
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Action = "Prototype – state to Approved/Active"
                });

                prototypeVersion.VersionState = VersionState.InApproval;
                prototypeVersion.ProposedState = state;
                prototypeVersion.ProposedDate = DateTimeOffset.Now;

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
            }
            else if ((prototypeVersion.VersionState == VersionState.Approved || prototypeVersion.VersionState == VersionState.Active) && state == VersionState.Obsolete && proposed)
            {
                var WorkFlow = prototypeVersion.Prototypes.PrototypeTypes.ObsolescenceWorkFlow;
                if (WorkFlow == null)
                    throw new InvalidOperationException($"Obsolescence workflow not found for prototype type {prototypeVersion.Prototypes.PrototypeTypes.Name} ({prototypeVersion.Prototypes.PrototypeType}).");

                var response = await _workFlowsService.CreateWorkFlowApplicationAsync(defaultIdentity, new CreateRequest
                {
                    WorkFlow = WorkFlow.Value,
                    Prototype = prototypeVersion.Prototype,
                    Version = prototypeVersion.Version,
                    Action = "Prototype – state to Obsolete"
                });

                prototypeVersion.ProposedState = state;
                prototypeVersion.ProposedDate = DateTimeOffset.Now;

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
        }
    }
}
