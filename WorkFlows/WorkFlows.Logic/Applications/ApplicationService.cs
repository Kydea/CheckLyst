using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.WorkFlows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Applications
{
    public class ApplicationService : IApplicationService
    {
        private readonly IWorkFlowsRepository _workFlowsRepository;

        public ApplicationService(IWorkFlowsRepository workFlowsRepository)
        {
            _workFlowsRepository = workFlowsRepository;
        }

        public async Task<GetStepsResponse> GetStepsAsync(DefaultIdentity identity, GetStepsRequest request)
        {
            var Application = await _workFlowsRepository.GetWorkFlowApplicationAsync(
                identity,
                request.Prototype,
                request.Version,
                request.Instance

            );

            return new GetStepsResponse
            {
                Action = Application.WorkFlowAction,

                Steps = Application.WorkFlowApplicationSteps.OrderBy(x => x.Step).ThenBy(x => x.ID).Select(x => new ApplicationStep
                {
                    ID = x.ID,
                    Step = x.Step + 1,
                    Date = x.Date,
                    User = x.User,
                    Approved = x.Approved,
                    Tags = string.Join(", ", x.WorkFlowApplicationStepTags.Select(t => t.Tag))
                }).ToList()
            };
        }
    }
}
