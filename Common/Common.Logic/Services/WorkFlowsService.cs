using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Common.Logic.Services
{
	public class WorkFlowsService : Repository, IWorkFlowsService
    {
        public WorkFlowsService(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public async Task<long> ApplyWorkFlowAsync(DefaultIdentity defaultIdentity, long workFlow, DateTimeOffset? date, long? prototype, int? version, long? instance, string action)
        {
            var WorkFlow = await Entities.WorkFlows.SingleOrDefaultAsync(x => x.Company == defaultIdentity.CompanyId && x.ID == workFlow);
            if (WorkFlow == null) throw new ArgumentException($"Work flow {defaultIdentity.CompanyId}.{workFlow} not found", nameof(workFlow));
            var WorkFlowApplication = new WorkFlowApplications
            {
                WorkFlow = workFlow,
                Date = date,

                Prototype = prototype,
                Version = version,

                Instance = instance,

                WorkFlowAction = action,
                Approved = null, //true means approved, false means rejected

                CreatedOn = DateTimeOffset.UtcNow,
                CreatedBy = defaultIdentity.Code
            };
            Entities.WorkFlowApplications.Add(WorkFlowApplication);
            Entities.WorkFlowApplicationSteps.AddRange(WorkFlow.WorkFlowSteps.OrderBy(x => x.Step).ThenBy(x => x.ID).Select(x => new WorkFlowApplicationSteps
            {
                WorkFlowApplications = WorkFlowApplication,
                WorkFlowStep = x.ID,
                Approved = false
            }));
            await SaveChangesAsync();
            return WorkFlowApplication.ID;
        }
	}
}
