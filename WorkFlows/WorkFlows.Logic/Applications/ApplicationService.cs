using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Authentication;
using CheckListManager.Common.Logic.WorkFlows;
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
        private readonly IAuthenticationService _authenticationService;

        public ApplicationService(IWorkFlowsRepository workFlowsRepository, IAuthenticationService authenticationService)
        {
            _workFlowsRepository = workFlowsRepository;
            _authenticationService = authenticationService;
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

        public async Task<SignResponse> SignAsync(DefaultIdentity identity, SignRequest request)
        {
            var SignResponse = new SignResponse();
            var Step = await _workFlowsRepository.GetWorkFlowApplicationStepAsync(identity, request.ApplicationStep);
            var Steps = Step.WorkFlowApplications.WorkFlowApplicationSteps;
            var FirstSignature = Steps.All(r => r.User == null);

            if (Step.User != null)
                throw new Exception("Signature already issued");
            if (Steps.Any(r => r.User == null && r.Step < Step.Step))
                throw new Exception("The previous step is not completed");
            if (Steps.Any(r => r.User == identity.Code && r.Step == Step.Step))
                throw new Exception("You have already signed for this step");
            if (Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved && request.ValidFrom < DateTime.UtcNow && request.Approved)
                throw new Exception("The prototype has a past validity start date");
            if (!request.Approved && String.IsNullOrWhiteSpace(request.Comment))
                throw new Exception("The comment is mandatory for the rejections");

            await _authenticationService.AuthenticateAsync(identity.CompanyId, identity.Code, request.Password);

            Step.Approved = SignResponse.Approved = request.Approved;
            Step.WorkFlowApplications.ModifiedBy = Step.User = identity.Code;
            Step.WorkFlowApplications.ModifiedOn = Step.Date = DateTimeOffset.UtcNow;
            Step.Comment = request.Comment;

            var PrototypeVersion = Step.WorkFlowApplications.PrototypeVersions;
            var Instance = Step.WorkFlowApplications.Instances;

            if (PrototypeVersion != null)
            {
                var Granted = false;
                foreach (var Tag in Step.WorkFlowApplicationStepTags)
                    if (identity.HasPermission("Prototypes", "Approve", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType, Tag.Tag))
                    {
                        Granted = true;
                        break;
                    }

                if (!Granted)
                    throw new Exception("Permission denied");
            }
            else if (Instance != null)
            {
                var Granted = false;
                foreach (var Tag in Step.WorkFlowApplicationStepTags)
                    if (identity.HasPermission("Prototypes", "Approve", Instance.Prototypes.Department, Instance.Prototypes.PrototypeType, Tag.Tag))
                    {
                        Granted = true;
                        break;
                    }

                if (!Granted)
                    throw new Exception("Permission denied");
            }
            else
                throw new InvalidOperationException();

            if (request.Approved)
            {
                bool AllApproved = Steps.All(x => x.Approved == true);

                if (PrototypeVersion != null)
                {
                    if (FirstSignature)
                    {
                        if (Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved)
                            PrototypeVersion.ValidFrom = request.ValidFromNow ? DateTimeOffset.UtcNow : request.ValidFrom.Value;

                        PrototypeVersion.ValidUntil = request.ValidUntilNow ? DateTimeOffset.UtcNow : request.ValidUntil;
                    }

                    if (AllApproved)
                    {
                        PrototypeVersion.ProposedState = null;
                        PrototypeVersion.ProposedDate = null;
                        //TODO Cambio di stato da servizio, deve:
                        //1. creare log
                        //2. creare history
                        //3. mettere a obsolete eventuale vecchia versione active
                        //4. cancellare le simulation evidence
                        //5. notificare per email
                        PrototypeVersion.VersionState =
                            PrototypeVersion.VersionState == VersionState.InSumulation ?
                                PrototypeVersion.ValidFrom.Value >= DateTimeOffset.UtcNow ? VersionState.Active : VersionState.Approved :
                                PrototypeVersion.ValidUntil.Value >= DateTimeOffset.UtcNow ? VersionState.Obsolete : PrototypeVersion.VersionState;
                    }
                }
                else if (Instance != null)
                {
                    throw new NotImplementedException();
                    //TODO Instances section
                }
            }
            else 
            {
                Step.WorkFlowApplications.Approved = false;
                if (PrototypeVersion != null)
                {
                    PrototypeVersion.ProposedState = null;
                    PrototypeVersion.ProposedDate = null;
                    //TODO Cambio di stato da servizio
                    PrototypeVersion.VersionState = VersionState.Draft;
                }
                else if (Instance != null)
                {
                    throw new NotImplementedException();
                    //TODO Instances section
                }
            }

            await _workFlowsRepository.SaveChangesAsync();

            if (PrototypeVersion != null)
            {
                SignResponse.State = PrototypeVersion.VersionState;
                SignResponse.ValidFrom = PrototypeVersion.ValidFrom;
                SignResponse.ValidUntil = PrototypeVersion.ValidUntil;
                SignResponse.ModifiedOn = PrototypeVersion.ModifiedOn;
                SignResponse.ModifiedBy = PrototypeVersion.ModifiedBy;
            }
            else if (Instance != null)
            {
                throw new NotImplementedException();
                //TODO Instances section
            }

            return SignResponse;
        }

        public async Task<CreateResponse> CreateAsync(DefaultIdentity identity, CreateRequest request)
        {
            return new CreateResponse
            {
                ID = await _workFlowsRepository.CreateWorkFlowApplication(identity, request.WorkFlow, request.Date, request.Prototype, request.Version, request.Instance, request.Action)
            };
        }

    }
}
