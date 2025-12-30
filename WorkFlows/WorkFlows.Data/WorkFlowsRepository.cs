using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Core.Exceptions;

namespace CheckListManager.WorkFlows.Data
{
    public class WorkFlowsRepository : Repository, IWorkFlowsRepository
    {
        public WorkFlowsRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public async Task<PagingResponse<List<TResult>>> GetWorkFlowsAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Common.Data.WorkFlows, TResult>> select,
            Expression<Func<Common.Data.WorkFlows, bool>> filters,
            Expression<Func<Common.Data.WorkFlows, TKey>> order,
            int currentPage,
            int itemsPerPage)
        {
            var Data = await Entities.WorkFlows
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            int Count = await Entities.WorkFlows
                .Where(filters)
                .CountAsync();

            return new PagingResponse<List<TResult>>
            {
                Data = Data,
                Count = Count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }
        public async Task<Common.Data.WorkFlows> GetWorkFlowAsync(IDefaultIdentity identity, long id)
        {
            var WorkFlow = await Entities.WorkFlows.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id) ?? throw new DatabaseException($"Workflow {id} not found");
            return WorkFlow;
        }

        public async Task<Common.Data.WorkFlows> AddWorkFlowAsync(IDefaultIdentity identity, Common.Data.WorkFlows workFlow)
        {
            workFlow.Company = identity.CompanyId;
            workFlow.CreatedOn = DateTimeOffset.UtcNow;
            workFlow.CreatedBy = identity.Code;
            Entities.WorkFlows.Add(workFlow);
            await SaveChangesAsync();
            return workFlow;
        }

        public async Task<Common.Data.WorkFlows> EditWorkFlowAsync(IDefaultIdentity identity, Common.Data.WorkFlows workFlow)
        {
            var ExistingPrototypeType = await GetWorkFlowAsync(identity, workFlow.ID);
            ExistingPrototypeType.Description = workFlow.Description;
            ExistingPrototypeType.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingPrototypeType.ModifiedBy = identity.Code;
            Entities.WorkFlowSteps.RemoveRange(ExistingPrototypeType.WorkFlowSteps);
            ExistingPrototypeType.WorkFlowSteps = workFlow.WorkFlowSteps;
            await SaveChangesAsync();
            return ExistingPrototypeType;
        }

        public async Task DeleteWorkFlowAsync(IDefaultIdentity identity, long id)
        {
            Entities.WorkFlows.Remove(await GetWorkFlowAsync(identity, id));
            await SaveChangesAsync();
        }

        public async Task<WorkFlowApplications> GetWorkFlowApplicationAsync(IDefaultIdentity identity, long? prototype, int? version, long? instance, long? report)
        {
            var Application = await Entities.WorkFlowApplications.SingleOrDefaultAsync(x => x.WorkFlows.Company == identity.CompanyId && (prototype != null && x.Prototype == prototype && x.Version == version || instance != null && x.Instance == instance || report != null && x.Report == report) && x.Approved == null)
                ?? throw new ArgumentException($"Workflow application for {(prototype != null ? $"prototype version {prototype}.{version}" : instance != null ? $"instance {instance}" : $"report {report}")} not found");
            return Application;
        }

        public async Task<WorkFlowApplicationSteps> GetWorkFlowApplicationStepAsync(IDefaultIdentity identity, long id)
        {
            var Step = await Entities.WorkFlowApplicationSteps.SingleOrDefaultAsync(x => x.Company == identity.CompanyId && x.ID == id) ?? throw new ArgumentException($"Workflow application step {id} not found");
            return Step;
        }

        public async Task<WorkFlowApplications> CreateWorkFlowApplication(IDefaultIdentity defaultIdentity, long workFlow, DateTimeOffset? date, long? prototype, int? version, long? instance, long? report, string action)
        {
            var PrototypeVersion = instance == null ? Entities.PrototypeVersions.Single(pv => pv.Prototype == prototype.Value && pv.Version == version.Value) : null;
            var Instance = instance == null ? null : Entities.Instances.Single(i => i.ID == instance.Value);

            if (
                Instance != null 
                    && !defaultIdentity.HasPermission("Instances", "Edit", Instance.Prototypes.Department, Instance.Prototypes.PrototypeType)
                    && !defaultIdentity.HasPermission("Instances", "Edit", Instance.Prototypes.Department)
                || PrototypeVersion != null 
                    && !defaultIdentity.HasPermission("Prototypes", "Preview", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
                    && !defaultIdentity.HasPermission("Prototypes", "Preview", PrototypeVersion.Prototypes.Department)
                || report.HasValue
                    && !defaultIdentity.HasPermission("Report", "Edit")
            )
                throw new Exception("Access denied");

            var WorkFlow = await Entities.WorkFlows.SingleOrDefaultAsync(x => x.Company == defaultIdentity.CompanyId && x.ID == workFlow) ?? throw new ArgumentException($"Workflow {defaultIdentity.CompanyId}.{workFlow} not found", nameof(workFlow));

            WorkFlowApplications WorkFlowApplication = null;

            if (Instance != null)
                WorkFlowApplication = await Entities.WorkFlowApplications.SingleOrDefaultAsync(a => a.Instance == Instance.ID && a.Approved == null);
            else if (PrototypeVersion != null)
                WorkFlowApplication = await Entities.WorkFlowApplications.SingleOrDefaultAsync(a => a.Prototype == PrototypeVersion.Prototype && a.Version == PrototypeVersion.Version && a.Approved == null);
            else if (report.HasValue)
                WorkFlowApplication = await Entities.WorkFlowApplications.SingleOrDefaultAsync(a => a.Report == report.Value && a.Approved == null);

            if (WorkFlowApplication != null)
            {
                //security deletion of applications created on a client connection failure that would yield a unique index violation
                Entities.WorkFlowApplications.Remove(WorkFlowApplication);
                await SaveChangesAsync();
            }

            WorkFlowApplication = new WorkFlowApplications
            {
                WorkFlow = workFlow,
                Date = date,

                Prototype = prototype,
                Version = version,

                Instance = instance,

                Report = report,

                WorkFlowAction = action,
                Approved = null, //true means approved, false means rejected

                CreatedOn = DateTimeOffset.UtcNow,
                CreatedBy = defaultIdentity.Code
            };
            Entities.WorkFlowApplications.Add(WorkFlowApplication);
            Entities.WorkFlowApplicationSteps.AddRange(WorkFlow.WorkFlowSteps.OrderBy(x => x.Step).ThenBy(x => x.ID).Select(x => new WorkFlowApplicationSteps
            {
                Company = defaultIdentity.CompanyId,
                WorkFlowApplications = WorkFlowApplication,
                Step = x.Step,
                Reason = x.Reason,
                WorkFlowApplicationStepTags = x.WorkFlowStepTags.Select(t => new WorkFlowApplicationStepTags { Tag = t.Tag }).ToList()
            }));
            await SaveChangesAsync();
            Entities.Entry(WorkFlowApplication).Reference(c => c.PrototypeVersions).Load();
            Entities.Entry(WorkFlowApplication).Reference(c => c.Instances).Load();
            return WorkFlowApplication;
        }

        public void DeletePrototypeVersionInstances(PrototypeVersions prototypeVersion)
        {
            Entities.Instances.RemoveRange(prototypeVersion.Instances);
        }
    }
}
