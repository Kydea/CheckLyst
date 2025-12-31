using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Authentication;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.FTP;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.WorkFlows;
using CheckListManager.WorkFlows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;

namespace CheckListManager.WorkFlows.Logic.Applications
{
    public class ApplicationService : IApplicationService
    {
        private readonly IWorkFlowsRepository _workFlowsRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IQueueService _queueService;
        private readonly ISettingService _settingService;
        private readonly IConfigurationService _configurationService;
        private readonly ICommonRepository _commonRepository;

        public ApplicationService(IWorkFlowsRepository workFlowsRepository,
            IAuthenticationService authenticationService, IQueueService queueService, ISettingService settingService,
            IConfigurationService configurationService, ICommonRepository commonRepository)
        {
            _workFlowsRepository = workFlowsRepository;
            _authenticationService = authenticationService;
            _queueService = queueService;
            _settingService = settingService;
            _configurationService = configurationService;
            _commonRepository = commonRepository;
        }

        public async Task<GetStepsResponse> GetStepsAsync(DefaultIdentity identity, GetStepsRequest request)
        {
            var Application = await _workFlowsRepository.GetWorkFlowApplicationAsync(
                identity,
                request.Prototype,
                request.Version,
                request.Instance,
                request.Report
            );

            if (
                Application.Instances != null
                && !identity.HasPermission("Instances", "View", Application.Instances.Prototypes.Department, Application.Instances.Prototypes.PrototypeType)
                && !identity.HasPermission("Instances", "View", Application.Instances.Prototypes.Department)
                || Application.PrototypeVersions != null
                && !identity.HasPermission("Prototypes", "View", Application.PrototypeVersions.Prototypes.Department, Application.PrototypeVersions.Prototypes.PrototypeType)
                && !identity.HasPermission("Prototypes", "View", Application.PrototypeVersions.Prototypes.Department)
                || request.Report.HasValue
                && !identity.HasPermission("Reports", "Edit")
            )
                throw new Exception("Access denied");

            return new GetStepsResponse
            {
                Action = Application.WorkFlowAction,

                Steps = Application.WorkFlowApplicationSteps.OrderBy(x => x.Step).ThenBy(x => x.ID).Select(x =>
                    new ApplicationStep
                    {
                        ID = x.ID,
                        Step = x.Step + 1,
                        Reason = x.Reason,
                        Date = x.Date,
                        User = x.User == null ? "" : x.User + " - " + x.UserDescription,
                        Approved = x.Approved,
                        Tags = string.Join(", ", x.WorkFlowApplicationStepTags.Select(t => t.Tag))
                    }).ToList()
            };
        }

        private async Task SetState(DefaultIdentity identity, PrototypeVersions PrototypeVersion, string State, DateTimeOffset Now)
        {
            PrototypeVersion.ProposedState = null;
            PrototypeVersion.ProposedDate = null;
            if (PrototypeVersion.VersionState != State)
            {
                PrototypeVersion.VersionState = State;
                if (PrototypeVersion.VersionState == VersionState.Obsolete && PrototypeVersion.ValidUntil == null)
                    PrototypeVersion.ValidUntil = Now;

                await _commonRepository.AddTrailRecordAsync(new Records
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.PrototypeStateChanged,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}",
                    CheckListID = PrototypeVersion.Prototype,
                    Information = State
                });
            }
        }

        private async Task<bool> Approve(DefaultIdentity identity, PrototypeVersions PrototypeVersion)
        {
            var UtcNow = DateTimeOffset.UtcNow;
            var Export = false;

            if (!PrototypeVersion.ValidFrom.HasValue)
                PrototypeVersion.ValidFrom = UtcNow;

            var State =
                PrototypeVersion.VersionState == VersionState.InApproval ? PrototypeVersion.ValidFrom > UtcNow ? VersionState.Approved : VersionState.Active :
                PrototypeVersion.ValidUntil > UtcNow ? PrototypeVersion.VersionState : VersionState.Obsolete;

            if (State == VersionState.Active)
            {
                PrototypeVersion.Prototypes.Description = PrototypeVersion.Description;
                PrototypeVersion.Prototypes.ModifiedOn = PrototypeVersion.ModifiedOn;
                PrototypeVersion.Prototypes.ModifiedBy = PrototypeVersion.ModifiedBy;

                var OldVersion = PrototypeVersion.Prototypes.PrototypeVersions.SingleOrDefault(pv => pv.VersionState == VersionState.Active);
                if (OldVersion != null)
                {
                    await SetState(identity, OldVersion, VersionState.Obsolete, UtcNow);
                    OldVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
                    {
                        Date = DateTimeOffset.UtcNow,
                        State = VersionState.Obsolete,
                        User = identity.Code
                    });
                }
            }

            if (State == VersionState.Active || State == VersionState.Obsolete) Export = true;
            await SetState(identity, PrototypeVersion, State, UtcNow);
            return Export;
        }

        private async Task Export(DefaultIdentity identity, PrototypeVersions PrototypeVersion)
        {
            await _queueService.SendObjectAsync(QueueName.PrototypesFTP, new PrototypesFTPMessage
            {
                Company = identity.CompanyId,
                Prototype = PrototypeVersion.Prototype,
                PrototypeName = PrototypeVersion.Prototypes.Name,
                Version = PrototypeVersion.Version,
                State = PrototypeVersion.VersionState,
                Path = $"{PrototypeVersion.Prototypes.Companies.Name?.Replace('#', '-') ?? ""}#{PrototypeVersion.Prototypes.Departments.Name?.Replace('#', '-') ?? ""}#{PrototypeVersion.Prototypes.PrototypeTypes.Name?.Replace('#', '-') ?? ""}#{(PrototypeVersion.Prototypes.Name?.Replace('#', '-') ?? "")}#{PrototypeVersion.Version}#{PrototypeVersion.Description?.Replace('#', '-') ?? ""}"
            });
        }

        private async Task Reject(DefaultIdentity identity, PrototypeVersions PrototypeVersion)
        {
            if (PrototypeVersion.VersionState == VersionState.InApproval)
                _workFlowsRepository.DeletePrototypeVersionInstances(PrototypeVersion);
            await SetState(identity, PrototypeVersion,
                PrototypeVersion.VersionState == VersionState.InApproval
                    ? VersionState.Draft
                    : PrototypeVersion.VersionState, DateTimeOffset.UtcNow);
        }

        private async Task SetState(DefaultIdentity identity, Instances Instance, string State)
        {
            Instance.InstanceState = State;

            await _commonRepository.AddTrailRecordAsync(new Records
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceStateChanged,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = $"{Instance.Prototypes.Name}.{Instance.Version}.{Instance.Year}.{Instance.Number}",
                CheckListID = Instance.ID,
                Information = JsonConvert.SerializeObject(new { State })
            });
        }

        private async Task<bool> Approve(DefaultIdentity identity, Instances Instance)
        {
            var Export = false;
            await SetState(identity, Instance, InstanceState.Confirmed);

            if (!string.IsNullOrWhiteSpace(Instance.FileName)) Export = true;
            return Export;
        }

        private async Task Export(DefaultIdentity identity, Instances Instance)
        {
            await _queueService.SendObjectAsync(QueueName.InstancesFTP, new InstancesFTPMessage
            {
                Company = identity.CompanyId,
                ID = Instance.ID,
                Path = Instance.FileName,
                Code = $"{Instance.PrototypeName}.{Instance.Version}.{Instance.Year}.{Instance.Number}"
            });
        }

        private async Task Reject(DefaultIdentity identity, Instances Instance)
        {
            await SetState(identity, Instance, InstanceState.Deleted);
        }

        private async Task SetState(DefaultIdentity identity, Reports Report, string State)
        {
            Report.ReportState = State;
            Report.ProposedState = null;
            Report.ProposedDate = null;

            await _commonRepository.AddTrailRecordAsync(new Records
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = TrailAction.ReportUpdated,
                Date = DateTimeOffset.UtcNow,
                Information = JsonConvert.SerializeObject(new { State })
            });
        }

        private Task Approve(DefaultIdentity identity, Reports Report) =>
            SetState(identity, Report, Report.ReportState == ReportState.Confirmed ? ReportState.Obsolete : ReportState.Confirmed);

        private async Task Reject(DefaultIdentity identity, Reports Report) 
        {
            await SetState(identity, Report, Report.ReportState == ReportState.Confirmed ? ReportState.Confirmed : ReportState.Draft);
        }

        public async Task<SignResponse> SignAsync(DefaultIdentity identity, SignRequest request, string timeZone)
        {
            PrototypeVersions PrototypeVersion = null;
            Instances Instance = null;
            Reports Report = null;

            var SignResponse = new SignResponse();
            try
            {
                var Step = await _workFlowsRepository.GetWorkFlowApplicationStepAsync(identity, request.ApplicationStep);
                var Steps = Step.WorkFlowApplications.WorkFlowApplicationSteps;
                var FirstSignature = Steps.All(r => r.User == null);

                PrototypeVersion = Step.WorkFlowApplications.PrototypeVersions;
                Instance = Step.WorkFlowApplications.Instances;
                Report = Step.WorkFlowApplications.Reports;

                if (Step.User != null)
                    throw new Exception("Signature already issued");
                if (Steps.Any(r => r.User == null && r.Step < Step.Step))
                    throw new Exception("The previous step is not completed");
                if (Steps.Any(r => r.User == identity.Code && r.Step == Step.Step))
                    throw new Exception("You have already signed for this step");
                if (
                    Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved
                    && request.ValidFrom < DateTime.UtcNow
                    && request.Approved
                )
                    throw new Exception("The prototype has a past validity start date");
                if (
                    Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved
                    && (request.ValidFrom >= request.ValidUntil || request.ValidUntil < DateTimeOffset.UtcNow)
                    && request.Approved
                )
                    throw new Exception("The prototype has a past validity end date");
                if (!request.Approved && string.IsNullOrWhiteSpace(request.Comment))
                    throw new Exception("The comment is mandatory for the rejections");

                await _authenticationService.AuthenticateAsync(identity.Code, request.Password);

                Step.Approved = SignResponse.Approved = request.Approved;
                Step.WorkFlowApplications.ModifiedBy = Step.User = identity.Code;
                Step.UserDescription = identity.Description;
                Step.WorkFlowApplications.ModifiedOn = Step.Date = DateTimeOffset.UtcNow;
                Step.Comment = request.Comment;

                if (PrototypeVersion != null)
                {
                    var Granted = false;
                    if (
                        identity.HasPermission("Prototypes", "Approve", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
                        || identity.HasPermission("Prototypes", "Approve", PrototypeVersion.Prototypes.Department)
                    )
                        foreach (var Tag in Step.WorkFlowApplicationStepTags)
                            if (
                                identity.HasPermission("PrototypeWorkFlowLevel", null, PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType, Tag.Tag)
                                || identity.HasPermission("PrototypeWorkFlowLevel", null, PrototypeVersion.Prototypes.Department, null, Tag.Tag)
                                || identity.HasPermission("PrototypeWorkFlowLevel", null, null, null, Tag.Tag)
                            )
                            {
                                Granted = true;
                                break;
                            }

                    if (!Granted) throw new Exception("Permission denied");
                }
                else if (Instance != null)
                {
                    var Granted = false;
                    if (
                        identity.HasPermission("Instances", "Confirm", Instance.Prototypes.Department, Instance.Prototypes.PrototypeType)
                        || identity.HasPermission("Instances", "Confirm", Instance.Prototypes.Department)
                    )
                        foreach (var Tag in Step.WorkFlowApplicationStepTags)
                            if (
                                identity.HasPermission("InstanceWorkFlowLevel", null, Instance.Prototypes.Department, Instance.Prototypes.PrototypeType, Tag.Tag)
                                || identity.HasPermission("InstanceWorkFlowLevel", null, Instance.Prototypes.Department, null, Tag.Tag)
                                || identity.HasPermission("InstanceWorkFlowLevel", null, null, null, Tag.Tag)
                            )
                            {
                                Granted = true;
                                break;
                            }

                    if (!Granted) throw new Exception("Permission denied");
                }
                else if (Report != null)
                {
                    var Granted = false;
                    foreach (var Tag in Step.WorkFlowApplicationStepTags)
                        if (identity.HasPermission("ReportWorkFlowLevel", null, null, null, Tag.Tag))
                        {
                            Granted = true;
                            break;
                        }

                    if (!Granted) throw new Exception("Permission denied");
                }
                else
                    throw new InvalidOperationException();

                bool ExportFTP = false;

                if (request.Approved)
                {
                    bool AllApproved = SignResponse.Completed = Steps.All(x => x.Approved == true);

                    if (PrototypeVersion != null)
                    {
                        if (FirstSignature)
                        {
                            if (Step.WorkFlowApplications.WorkFlowAction == WorkFlowAction.PrototypeToApproved)
                                PrototypeVersion.ValidFrom = request.ValidFromNow ? default(DateTimeOffset?) : request.ValidFrom.Value;

                            PrototypeVersion.ValidUntil = request.ValidUntilNow ? null : request.ValidUntil;
                        }

                        if (AllApproved)
                        {
                            ExportFTP = await Approve(identity, PrototypeVersion);
                            Step.WorkFlowApplications.FinalState = PrototypeVersion.VersionState;
                        }
                    }
                    else if (Instance != null)
                    {
                        if (AllApproved)
                        {
                            ExportFTP = await Approve(identity, Instance);
                            Step.WorkFlowApplications.FinalState = Instance.InstanceState;
                        }
                    }
                    else if (Report != null)
                    {
                        if (AllApproved)
                        {
                            await Approve(identity, Report);
                            Step.WorkFlowApplications.FinalState = Report.ReportState;
                        }
                    }

                    if (AllApproved)
                        Step.WorkFlowApplications.Approved = true;
                    else
                        await SendEmailsAsync(identity, Step.WorkFlowApplications);
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
                    else if (Report != null)
                    {
                        await Reject(identity, Report);
                        Step.WorkFlowApplications.FinalState = Report.ReportState;
                    }
                }

                await _workFlowsRepository.SaveChangesAsync();

                if (ExportFTP)
                {
                    if (PrototypeVersion != null)
                        await Export(identity, PrototypeVersion);
                    else if (Instance != null)
                        await Export(identity, Instance);
                }

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
                else if (Report != null)
                    SignResponse.State = Report.ReportState;

                var checklistCode =
                        PrototypeVersion != null ? $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}" :
                        Instance != null ? $"{Instance.PrototypeName}.{Instance.Version}.{Instance.Year}.{Instance.Number}" :
                        null;
                var information = new Dictionary<string, object>
                {
                    { "Action", request.Approved ? "Approve" : "Reject" },
                    { "Comment", request.Comment }
                };

                if (PrototypeVersion != null)
                {
                    information.Add("ValidFrom", SignResponse.ValidFrom?.ToLocalTime(timeZone) ?? (object)"(now)");
                    information.Add("ValidUntil", SignResponse.ValidUntil?.ToLocalTime(timeZone) ?? (object)"(never expires)");
                }

                await _commonRepository.AddTrailRecordAsync(new Records
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.WorkflowSignatureIssued,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = checklistCode,
                    CheckListID = Instance?.ID ?? PrototypeVersion?.Prototype,
                    Information = information.ToJson(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None })
                });

                return SignResponse;
            }
            catch (Exception e)
            {
                var checklistCode =
                    PrototypeVersion != null ? $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}" :
                    Instance != null ? $"{Instance.PrototypeName}.{Instance.Version}.{Instance.Year}.{Instance.Number}" :
                    null;
                var information = new Dictionary<string, object>
                {
                    { "Action", request.Approved ? "Approve" : "Reject" },
                    { "Comment", request.Comment },
                    { "ErrorMessage", e.Message }
                };

                if (PrototypeVersion != null)
                {
                    information.Add("ValidFrom", SignResponse.ValidFrom?.ToLocalTime(timeZone) ?? (object)"(now)");
                    information.Add("ValidUntil", SignResponse.ValidUntil?.ToLocalTime(timeZone) ?? (object)"(never expires)");
                }

                await _commonRepository.AddTrailRecordAsync(new Records
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.WorkflowSignatureFailed,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = checklistCode,
                    CheckListID = Instance?.ID ?? PrototypeVersion?.Prototype,
                    Information = information.ToJson(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None })
                });

                throw;
            }
        }

        public async Task<CreateResponse> CreateAsync(DefaultIdentity identity, CreateRequest request)
        {
            var workFlowApplication = await _workFlowsRepository.CreateWorkFlowApplication(identity, request.WorkFlow, request.Date, request.Prototype, request.Version, request.Instance, request.Report, request.Action);
            await SendEmailsAsync(identity, workFlowApplication);

            return new CreateResponse
            {
                ID = workFlowApplication.ID
            };
        }

        private async Task SendEmailsAsync(IDefaultIdentity identity, WorkFlowApplications workFlowApplication)
        {
            var CurrentStep = workFlowApplication.WorkFlowApplicationSteps.Where(s => s.Approved == null).Min(s => s.Step);
            foreach (var step in workFlowApplication.WorkFlowApplicationSteps.Where(s => s.Step == CurrentStep))
                await SendEmailsAsync(identity, step);
        }

        private async Task SendEmailsAsync(IDefaultIdentity identity, WorkFlowApplicationSteps workFlowApplicationStep)
        {
            var Instance = workFlowApplicationStep.WorkFlowApplications.Instances;
            var PrototypeVersion = workFlowApplicationStep.WorkFlowApplications.PrototypeVersions;
            var Report = workFlowApplicationStep.WorkFlowApplications.Reports;
            var Users =
                PrototypeVersion != null ? await _commonRepository.GetAppointedUsersAsync
                (
                    PrototypeVersion.Prototypes.Company, "Prototypes", "Approve",
                    PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType, "PrototypeWorkFlowLevel",
                    workFlowApplicationStep.WorkFlowApplicationStepTags.Select(t => t.Tag)
                ) :
                Instance != null ? await _commonRepository.GetAppointedUsersAsync
                (
                    Instance.Company, "Instances", "Confirm",
                    Instance.Prototypes.Department, Instance.Prototypes.PrototypeType, "InstanceWorkFlowLevel",
                    workFlowApplicationStep.WorkFlowApplicationStepTags.Select(t => t.Tag)
                ) :
                await _commonRepository.GetAppointedUsersAsync
                (
                    identity.CompanyId, workFlowApplicationStep.WorkFlowApplicationStepTags.Select(t => t.Tag)
                );
            var webApplicationUrl = _configurationService.WebApplicationUrl;

            foreach (var user in Users)
                if (!string.IsNullOrEmpty(user.Email))
                {
                    var message = await _settingService.SignatureMessageAsync(user.Company);
                    var url =
                        PrototypeVersion != null ? $"{webApplicationUrl}/Prototypes/Detail/{PrototypeVersion.Prototypes.ID}/{PrototypeVersion.Version}" :
                        Instance != null ? $"{webApplicationUrl}/Instances/Detail/{Instance.Code}" :
                        $"{webApplicationUrl}/Reports/Editor/{Report.ID}";
                    var label =
                        PrototypeVersion != null ? $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}" :
                        Instance != null ? $"{Instance.PrototypeName}.{Instance.Version}.{Instance.Year}.{Instance.Number}" :
                        Report.Code;
                    await _queueService.SendObjectAsync(QueueName.Email,
                        new EmailMessage(user.Company, "Signature needed", message, user.Email, new List<Tuple<string, string>>
                            {
                                new Tuple<string, string>("User", user.Description),
                                new Tuple<string, string>("CheckListType", PrototypeVersion != null ? "prototype" : Instance != null ? "instance" : "report"),
                                new Tuple<string, string>("CheckListCode", $"<a href='{url}' target='_blank'>{label}</a>"),
                            }
                        )
                        {
                            TrailMessage = new EmailTrailMessage(identity.Code, label, Instance?.ID ?? PrototypeVersion?.Prototype, null)
                        });
                }
        }
    }
}