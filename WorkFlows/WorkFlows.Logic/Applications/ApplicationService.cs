using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Authentication;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
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
        private readonly IQueueService _queueService;

        public ApplicationService(IWorkFlowsRepository workFlowsRepository, IAuthenticationService authenticationService, IQueueService queueService)
        {
            _workFlowsRepository = workFlowsRepository;
            _authenticationService = authenticationService;
            _queueService = queueService;
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

        private async Task SetPrototypeVersionState(DefaultIdentity identity, PrototypeVersions PrototypeVersion, string State)
        {
            PrototypeVersion.ProposedState = null;
            PrototypeVersion.ProposedDate = null;
            if (PrototypeVersion.VersionState != State)
            {
                PrototypeVersion.VersionState = State;

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.PrototypeStateChanged,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}",
                    Information = State
                });
            }
        }

        private async Task Approve(DefaultIdentity identity, PrototypeVersions PrototypeVersion)
        {
            //TODO
            //5. notificare per email

            var UtcNow = DateTimeOffset.UtcNow;

            if (!PrototypeVersion.ValidFrom.HasValue)
                PrototypeVersion.ValidFrom = UtcNow;

            var State = 
                PrototypeVersion.VersionState == VersionState.InApproval ?
                    PrototypeVersion.ValidFrom > UtcNow ? VersionState.Approved : VersionState.Active :
                    PrototypeVersion.ValidUntil > UtcNow ? PrototypeVersion.VersionState : VersionState.Obsolete;

            if (State == VersionState.Active)
            {
                PrototypeVersion.Prototypes.Description = PrototypeVersion.Description;
                PrototypeVersion.Prototypes.ModifiedOn = PrototypeVersion.ModifiedOn;
                PrototypeVersion.Prototypes.ModifiedBy = PrototypeVersion.ModifiedBy;

                var OldVersion = PrototypeVersion.Prototypes.PrototypeVersions.SingleOrDefault(pv => pv.VersionState == VersionState.Active);
                if (OldVersion != null) await SetPrototypeVersionState(identity, OldVersion, VersionState.Obsolete);
            } 
            await SetPrototypeVersionState(identity, PrototypeVersion, State);
        }

        private async Task Reject(DefaultIdentity identity, PrototypeVersions PrototypeVersion)
        {
            //TODO
            //5. notificare per email
            if (PrototypeVersion.VersionState == VersionState.InApproval)
                _workFlowsRepository.DeletePrototypeVersionInstances(PrototypeVersion);
            await SetPrototypeVersionState(identity, PrototypeVersion, PrototypeVersion.VersionState == VersionState.InApproval ? VersionState.Draft : PrototypeVersion.VersionState);
        }

        private async Task SetInstanceState(DefaultIdentity identity, Instances Instance, string State)
        {
            Instance.InstanceState = State;

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceStateChanged,
                Date = DateTimeOffset.UtcNow,
                CheckList = $"{Instance.Prototypes.Name}.{Instance.Version}.{Instance.Year}.{Instance.Number}",
                Information = State
            });
        }

        private async Task Approve(DefaultIdentity identity, Instances Instance)
        {
            //TODO
            //5. notificare per email
            await SetInstanceState(identity, Instance, InstanceState.Confirmed);
        }

        private async Task Reject(DefaultIdentity identity, Instances Instance)
        {
            //TODO
            //5. notificare per email
            await SetInstanceState(identity, Instance, InstanceState.Deleted);
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
            if (Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved && (request.ValidFrom >= request.ValidUntil || request.ValidUntil < DateTimeOffset.UtcNow) && request.Approved)
                throw new Exception("The prototype has a past validity end date");
            if (!request.Approved && String.IsNullOrWhiteSpace(request.Comment))
                throw new Exception("The comment is mandatory for the rejections");

            await _authenticationService.AuthenticateAsync(identity.Code, request.Password);

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
                    if (
                        identity.HasPermission("Prototypes", "Approve", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType, Tag.Tag)
                        || identity.HasPermission("Prototypes", "Approve", PrototypeVersion.Prototypes.Department, null, Tag.Tag)
                    )
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
                    if (
                        identity.HasPermission("Instances", "Confirm", Instance.Prototypes.Department, Instance.Prototypes.PrototypeType, Tag.Tag)
                        || identity.HasPermission("Instances", "Confirm", Instance.Prototypes.Department, null, Tag.Tag)
                    )
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
                bool AllApproved = SignResponse.Completed = Steps.All(x => x.Approved == true);

                if (PrototypeVersion != null)
                {
                    if (FirstSignature)
                    {
                        if (Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved)
                            PrototypeVersion.ValidFrom = request.ValidFromNow ? (DateTimeOffset?)null : request.ValidFrom.Value;

                        PrototypeVersion.ValidUntil = request.ValidUntilNow ? null : request.ValidUntil;
                    }

                    if (AllApproved)
                    {
                        await Approve(identity, PrototypeVersion);
                        Step.WorkFlowApplications.FinalState = PrototypeVersion.VersionState;
                    }

                }
                else if (Instance != null)
                {
                    if (AllApproved)
                    {
                        await Approve(identity, Instance);
                        Step.WorkFlowApplications.FinalState = Instance.InstanceState;
                    }
                }

                if (AllApproved)
                    Step.WorkFlowApplications.Approved = true;
            }
            else 
            {
                Step.WorkFlowApplications.Approved = false;
                SignResponse.Completed = true;
                if (PrototypeVersion != null)
                {
                    await Reject(identity, PrototypeVersion);
                    Step.WorkFlowApplications.FinalState = PrototypeVersion.VersionState;
                }
                else if (Instance != null)
                {
                    await Reject(identity, Instance);
                    Step.WorkFlowApplications.FinalState = Instance.InstanceState;
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
                SignResponse.State = Instance.InstanceState;

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
