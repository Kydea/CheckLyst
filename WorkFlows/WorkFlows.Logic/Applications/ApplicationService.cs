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

        public async Task SignAsync(DefaultIdentity identity, SignRequest request)
        {
            var Step = await _workFlowsRepository.GetWorkFlowApplicationStepAsync(identity, request.ApplicationStep);
            var Steps = Step.WorkFlowApplications.WorkFlowApplicationSteps;
            var FirstSignature = Steps.All(r => r.User == null);

            if (Step.User != null)
                throw new Exception("Signature already issued");

            //TODO
            //if (!UserFunctions.Contains(db, Security.CurrentUser.Code, Version.Prototypes.PrototypeTypes.Company, Version.Prototypes.PrototypeTypes.Department, Version.Prototypes.PrototypeType, UserFunctions.PROTOTYPEAPPROVER))
            //    throw new Exception("User does not have a suitable profile to perform this action");
            //if (UserFunctions.GetSignatureLevel(db, Security.CurrentUser.Code, Version.Prototypes.PrototypeTypes.Company, Version.Prototypes.PrototypeTypes.Department, Version.Prototypes.PrototypeType) != Row.SignatureLevel)
            //    throw new Exception("User does not have the right signature level to perform this action");
            if (Steps.Any(r => r.User == null && r.Step < Step.Step))
                throw new Exception("The previous step is not completed");
            if (Steps.Any(r => r.User == identity.Code && r.Step == Step.Step))
                throw new Exception("You have already signed for this step");
            if (Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved && request.ValidFrom < DateTime.UtcNow && request.Approved)
                throw new Exception("The prototype has a past validity start date");
            if (!request.Approved && String.IsNullOrWhiteSpace(request.Comment))
                throw new Exception("The comment is mandatory for the rejections");

            //Security.VerifyPassword(db, Username, Password); //instances.CreateInstanceFromPrototypeAsync

            Step.Approved = request.Approved;
            Step.WorkFlowApplications.ModifiedBy = Step.User = identity.Code;
            Step.WorkFlowApplications.ModifiedOn = Step.Date = DateTimeOffset.UtcNow;
            Step.Comment = request.Comment;

            var PrototypeVersion = Step.WorkFlowApplications.PrototypeVersions;
            var Instance = Step.WorkFlowApplications.Instances;

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
                        //Cambio di stato da servizio, deve:
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
                else
                    throw new InvalidOperationException();
            }
            else 
            {
                Step.WorkFlowApplications.Approved = false;
                if (PrototypeVersion != null)
                {
                    PrototypeVersion.ProposedState = null;
                    PrototypeVersion.ProposedDate = null;
                }
                else if (Instance != null)
                {
                    throw new NotImplementedException();
                    //TODO Instances section
                }
                else
                    throw new InvalidOperationException();
            }
            
            await _workFlowsRepository.SaveChangesAsync();
        }
    }
}
