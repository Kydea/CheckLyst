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

        public async Task<PrototypeVersions> SetStateAsync(DefaultIdentity defaultIdentity, long prototype, int version, string state, bool proposed)
        {
            var PrototypeVersion = await Entities.PrototypeVersions.SingleOrDefaultAsync(x => x.Prototypes.Company == defaultIdentity.CompanyId && x.Prototype == prototype && x.Version == version);
            if (PrototypeVersion == null)
                throw new Exception($"Prototype version {prototype}.{version} not found");

            //todo controlli

            if (PrototypeVersion.VersionState == VersionState.Draft && state == VersionState.InSumulation && !proposed)
                PrototypeVersion.VersionState = state;
            else if (PrototypeVersion.VersionState == VersionState.InSumulation && state == VersionState.Draft && !proposed)
                PrototypeVersion.VersionState = state;
            else if (PrototypeVersion.VersionState == VersionState.InSumulation && state == VersionState.Approved && proposed)
            {
                var WorkFlow = PrototypeVersion.Prototypes.PrototypeTypes.ApprovationWorkFlow;
                if (WorkFlow == null)
                    throw new InvalidOperationException($"Approvation workflow not found for prototype type {PrototypeVersion.Prototypes.PrototypeTypes.Name} ({PrototypeVersion.Prototypes.PrototypeType}).");

                await _workFlowsService.CreateWorkFlowApplicationAsync(defaultIdentity, new CreateRequest
                {
                    WorkFlow = WorkFlow.Value,
                    Prototype = PrototypeVersion.Prototype,
                    Version = PrototypeVersion.Version,
                    Action = "Prototype – state to Approved/Active"
                });

                PrototypeVersion.ProposedState = state;
                PrototypeVersion.ProposedDate = DateTimeOffset.Now;
            }
            else if ((PrototypeVersion.VersionState == VersionState.Approved || PrototypeVersion.VersionState == VersionState.Active) && state == VersionState.Obsolete && proposed)
            {
                var WorkFlow = PrototypeVersion.Prototypes.PrototypeTypes.ObsolescenceWorkFlow;
                if (WorkFlow == null)
                    throw new InvalidOperationException($"Obsolescence workflow not found for prototype type {PrototypeVersion.Prototypes.PrototypeTypes.Name} ({PrototypeVersion.Prototypes.PrototypeType}).");

                await _workFlowsService.CreateWorkFlowApplicationAsync(defaultIdentity, new CreateRequest
                {
                    WorkFlow = WorkFlow.Value,
                    Prototype = PrototypeVersion.Prototype,
                    Version = PrototypeVersion.Version,
                    Action = "Prototype – state to Obsolete"
                });

                PrototypeVersion.ProposedState = state;
                PrototypeVersion.ProposedDate = DateTimeOffset.Now;
            }
            else
                throw new InvalidOperationException($"Cannot {(proposed ? "propose" : "change")} state from {PrototypeVersion.VersionState} to {state}");

            await SaveChangesAsync();
            return PrototypeVersion;
        }
    }
}
