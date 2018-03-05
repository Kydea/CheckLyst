using System.Threading.Tasks;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;
using System.Linq;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Prototypes.Data;
using CheckListManager.Common.Data;
using CheckListManager.Prototypes.Logic.Models;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.Services;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using System.Threading;
using NLog;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.FTP;

namespace CheckListManager.Prototypes.Logic.Services
{
    public class PrototypeService : IPrototypeService
    {
        private readonly CheckListJsonConverter _converter;
        private readonly IPrototypesRepository _prototypeRepository;
        private readonly IQueueService _queueService;
        private readonly ICommonRepository _commonRepository;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PrototypeService(CheckListJsonConverter converter, IPrototypesRepository prototypeRepository, IQueueService queueService, ICommonRepository commonRepository)
        {
            _converter = converter;
            _prototypeRepository = prototypeRepository;
            _queueService = queueService;
            _commonRepository = commonRepository;
        }

        public async Task<PrototypeResponse> OpenAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.ID, request.Version);
            if (
                !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType) &&
                !identity.HasPermission("Instances", "Create", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Instances", "Create", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            var Prototype = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converter);
            Prototype.Setup();

            return await Task.FromResult(new PrototypeResponse
            {
                ID = PrototypeVersion.Prototype,
                Version = PrototypeVersion.Version,
                CheckList = Prototype,
                ModifiedOn = PrototypeVersion.ModifiedOn,
                ModifiedBy = PrototypeVersion.ModifiedBy,
                Name = PrototypeVersion.Prototypes.Name,
                Description = PrototypeVersion.Description,
                ValidFrom = PrototypeVersion.ValidFrom,
                ValidUntil = PrototypeVersion.ValidUntil,
                InstanceConfirmationWorkFlow = PrototypeVersion.InstanceConfirmationWorkFlow,
                State = PrototypeVersion.VersionState,
                Department = PrototypeVersion.Prototypes.Department,
                DepartmentName = PrototypeVersion.Prototypes.Departments.Name,
                PrototypeType = PrototypeVersion.Prototypes.PrototypeType,
                PrototypeTypeName = PrototypeVersion.Prototypes.PrototypeTypes.Name,
                ProposedState = PrototypeVersion.ProposedState,
                ProposedDate = PrototypeVersion.ProposedDate
            });
        }

        public async Task<PrototypeResponse> CloneAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.ID, request.Version);
            if (
                !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            var Prototype = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converter);
            Prototype.Setup();

            return await Task.FromResult(new PrototypeResponse
            {
                ID = -1,
                Version = -1,
                CheckList = Prototype,
                InstanceConfirmationWorkFlow = PrototypeVersion.InstanceConfirmationWorkFlow,
                State = VersionState.Draft
            });
        }

        public async Task<PrototypeResponse> CreateAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var Prototype = new Prototype
            {
                Header = new Group { Description = "(header)" }
            };

            Page Page;
            Group Group;
            Prototype.Pages.Add(Page = new Page { Description = "(page)" });
            Page.Groups.Add(Group = new Group { Description = "(group)" });

            return await Task.FromResult(new PrototypeResponse
            {
                ID = -1,
                Version = -1,
                CheckList = Prototype,
                State = VersionState.Draft
            });
        }

        public async Task<PrototypeResponse> VersionAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.ID, request.Version);
            if (
                !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            var Prototype = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converter);
            Prototype.Setup();

            return await Task.FromResult(new PrototypeResponse
            {
                ID = PrototypeVersion.Prototype,
                Version = -1,
                CheckList = Prototype,
                Name = PrototypeVersion.Prototypes.Name,
                Description = PrototypeVersion.Description,
                PrototypeType = PrototypeVersion.Prototypes.PrototypeType,
                Department = PrototypeVersion.Prototypes.Department,
                InstanceConfirmationWorkFlow = PrototypeVersion.InstanceConfirmationWorkFlow,

                State = VersionState.Draft
            });
        }

        public VerifyExpressionsResponse VerifyExpressions(PrototypeRequest request)
        {
            request.CheckList.Setup();
            return new VerifyExpressionsResponse
            {
                Valid = request.CheckList.Expressions.Values.All(e => e.Error == null),
                Expressions = request.CheckList.Expressions
            };
        }

        private void AddTags(Dictionary<String, PrototypeVersionTags> Tags, PrototypeRequest request, ICheckListGroup Group)
        {
            if (Group != null)
                foreach (var Item in Group.Items)
                    if (Item is ICheckListGroup SubGroup)
                        AddTags(Tags, request, SubGroup);
                    else if (!string.IsNullOrWhiteSpace(Item.Description) && (Item.TypeName == "Field" || Item.TypeName == "Signature") && !Tags.ContainsKey(Item.Description))
                    {
                        var Tag = new PrototypeVersionTags
                        {
                            Prototype = request.ID,
                            Version = request.Version,
                            Code = Item.Description
                        };
                        if (Tag.Code.Length > 50) Tag.Code = Tag.Code.Substring(0, 50);
                        Tags[Item.Description] = Tag;
                    }
        }

        public List<PrototypeVersionTags> CreateTags(PrototypeRequest request)
        {
            var Tags = new Dictionary<String, PrototypeVersionTags>();
            AddTags(Tags, request, request.CheckList.Header);
            return Tags.Values.ToList();
        }

        public async Task<PrototypeResponse> SaveAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            request.CheckList.Setup();
            var UnvalidExpression = request.CheckList.Expressions.FirstOrDefault(e => e.Value.Error != null);
            if (UnvalidExpression.Key != null)
                throw new Exception($"The expression '{UnvalidExpression.Key}' contains the error: {UnvalidExpression.Value.Error}");

            if (String.IsNullOrWhiteSpace(request.Name)) throw new Exception("Specify a name");
            if (!request.Department.HasValue) throw new Exception("Specify a department");
            if (!request.PrototypeType.HasValue) throw new Exception("Specify a prototype type");
            if (
                !identity.HasPermission("Prototypes", "Edit", request.Department.Value) &&
                !identity.HasPermission("Prototypes", "Edit", request.Department.Value, request.PrototypeType.Value)
            )
                throw new UnauthorizedAccessException();

            bool
                NewPrototype = request.ID < 0,
                NewVersion = NewPrototype || request.Version < 0;

            var Now = DateTimeOffset.UtcNow;
            Common.Data.Prototypes Prototype;
            PrototypeVersions PrototypeVersion;

            if (NewPrototype)
            {
                Prototype = new Common.Data.Prototypes
                {
                    Name = request.Name,
                    Company = identity.CompanyId,
                    Department = request.Department.Value,
                    PrototypeType = request.PrototypeType.Value,
                    Description = request.Description,
                    CreatedOn = Now,
                    CreatedBy = identity.Code
                };
                _prototypeRepository.AddPrototype(Prototype);
            }
            else
                Prototype = await _prototypeRepository.GetPrototypeAsync(identity, request.ID);

            if (NewVersion)
            {
                Prototype.PrototypeVersions.Add(PrototypeVersion = new PrototypeVersions
                {
                    VersionState = VersionState.Draft,
                    Version = Prototype.PrototypeVersions.Count == 0 ? 1 : Prototype.PrototypeVersions.Max(v => v.Version) + 1,
                    CreatedOn = Now,
                    CreatedBy = identity.Code
                });

                PrototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
                {
                    Prototype = Prototype.ID,
                    Version = PrototypeVersion.Version,
                    Date = Now,
                    State = VersionState.Draft,
                    User = identity.Code
                });
            }
            else
            {
                PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.ID, request.Version);
                if (PrototypeVersion.VersionState != VersionState.Draft) throw new Exception($"The version {request.ID}.{request.Version} is not editable");
                if ((PrototypeVersion.ModifiedOn?.UtcDateTime.Ticks ?? 0) - (request.ModifiedOn?.UtcDateTime.Ticks ?? 0) > 10 || PrototypeVersion.ModifiedBy != request.ModifiedBy) throw new Exception($"Concurrency error: the prototype has already been modified by the user {request.ModifiedBy}"); //the db approximate some tick, we take 10 as maximum

                PrototypeVersion.ModifiedOn = Now;
                PrototypeVersion.ModifiedBy = identity.Code;
            }

            var CheckList = request.CheckList;
            Check(CheckList);

            PrototypeVersion.Description = request.Description;
            PrototypeVersion.Data = JsonConvert.SerializeObject(CheckList, _converter);
            PrototypeVersion.ValidFrom = request.ValidFrom;
            PrototypeVersion.ValidUntil = request.ValidUntil;
            PrototypeVersion.InstanceConfirmationWorkFlow = request.InstanceConfirmationWorkFlow;

            //Deletes the old tags and creates the new ones
            _prototypeRepository.DeletePrototypeVersionTags(PrototypeVersion.PrototypeVersionTags);
            var Tags = CreateTags(request);
            Tags.ForEach(t => PrototypeVersion.PrototypeVersionTags.Add(t));

            await _prototypeRepository.SaveChangesAsync();

            if (NewVersion)
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.PrototypeCreated,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}"
                });

            return new PrototypeResponse
            {
                ID = PrototypeVersion.Prototype,
                Version = PrototypeVersion.Version,
                ModifiedBy = PrototypeVersion.ModifiedBy,
                ModifiedOn = PrototypeVersion.ModifiedOn
            };
        }

        private void Check(Prototype CheckList)
        {
            foreach (Page Page in CheckList.Pages)
                Page.Check();
        }

        public async Task DeleteAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.ID, request.Version);
            if (
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            var CheckListCode = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}";

            await _prototypeRepository.DeletePrototypeVersionAsync(identity, request.ID, request.Version);

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = TrailAction.PrototypeDeleted,
                Date = DateTimeOffset.UtcNow,
                CheckList = CheckListCode
            });
        }

        public async Task<PrototypeResponse> SetStateAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.ID, request.Version);
            if (
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            await _prototypeRepository.SetStateAsync(identity, PrototypeVersion, request.State, request.StateProposed);

            if (request.StateProposed && request.State == VersionState.Approved) //we track both the state change and the propoed state for the wirkflow
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.PrototypeStateChanged,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}",
                    Information = VersionState.InApproval
                });

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = request.StateProposed ? TrailAction.PrototypeStateChangeProposed : TrailAction.PrototypeStateChanged,
                Date = DateTimeOffset.UtcNow,
                CheckList = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}",
                Information = request.State
            });

            return new PrototypeResponse
            {
                State = PrototypeVersion.VersionState,
                ProposedState = PrototypeVersion.ProposedState,
                ProposedDate = PrototypeVersion.ProposedDate
            };
        }

        public async Task<List<HistoryRow>> History(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.ID, request.Version);
            if (
                !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department)
                && !identity.HasPermission("Prototypes", "View", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            return await Task.FromResult(PrototypeVersion.PrototypeVersionsHistory.OrderBy(h => h.ID).Select(h => new HistoryRow
            {
                ID = h.ID,
                Date = h.Date,
                User = $"{h.User} - {_commonRepository.GetUser(identity.CompanyId, h.User)?.Description ?? "(unknown user)"}",
                HasWorkFlow = h.WorkFlowApplications != null,
                State = h.WorkFlowApplications == null ? h.State : h.WorkFlowApplications.FinalState,
                ProposedState = h.WorkFlowApplications == null ? null : h.State,
                Approved = h.WorkFlowApplications?.Approved,
                Steps = h.WorkFlowApplications?.WorkFlowApplicationSteps.OrderBy(s => s.Step).ThenBy(s => s.ID).Select(s => new HistoryStep
                {
                    ID = s.ID,
                    Step = s.Step + 1,
                    Date = s.Date,
                    User = s.User == null ? string.Empty : $"{s.User} - {_commonRepository.GetUser(identity.CompanyId, s.User)?.Description ?? "(unknown user)"}",
                    Level = $"({string.Join(", ", s.WorkFlowApplicationStepTags.Select(t => t.Tag))})",
                    Reason = s.Reason,
                    Comment = s.Comment,
                    Approved = s.Approved,
                    AppointedUser = s.User == null ? string.Join(", ", _commonRepository.GetAppointedUsers(identity.CompanyId, "Prototypes", "Approve", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType, s.WorkFlowApplicationStepTags.Select(t => t.Tag)).Select(u => u.Code)) : null
                }).ToList()
            }).ToList());
        }

        private async Task SetPrototypeVersionState(PrototypeVersions PrototypeVersion, string State)
        {
            PrototypeVersion.VersionState = State;

            PrototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
            {
                Prototype = PrototypeVersion.Prototype,
                Version = PrototypeVersion.Version,
                Date = PrototypeVersion.ModifiedOn.Value,
                State = State,
                User = "(system)"
            });

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = PrototypeVersion.Prototypes.Company,
                Action = TrailAction.PrototypeStateAutoChanged,
                User = "(system)",
                Date = PrototypeVersion.ModifiedOn.Value,
                CheckList = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}",
                Information = State
            });
        }

        public async Task UpdatePrototypeVersionsState(CancellationToken CancellationToken)
        {
            var PrototypeVersions = await _prototypeRepository.GetPrototypeVersionsAsync(pv =>
                pv.VersionState == VersionState.Approved && pv.ValidFrom <= DateTimeOffset.UtcNow
                || (pv.VersionState == VersionState.Approved || pv.VersionState == VersionState.Active) && pv.ValidUntil <= DateTimeOffset.UtcNow
            );
            foreach (var PrototypeVersion in PrototypeVersions)
                try
                {
                    if (CancellationToken.IsCancellationRequested) return;

                    var UtcNow = DateTimeOffset.UtcNow;

                    var State = PrototypeVersion.ValidUntil <= UtcNow ? VersionState.Obsolete : VersionState.Active;
                    PrototypeVersion.ModifiedOn = UtcNow;
                    PrototypeVersion.ModifiedBy = "(system)";

                    if (State == VersionState.Active)
                    {
                        PrototypeVersion.ValidFrom = UtcNow;

                        PrototypeVersion.Prototypes.Description = PrototypeVersion.Description;
                        PrototypeVersion.Prototypes.ModifiedOn = PrototypeVersion.ModifiedOn;
                        PrototypeVersion.Prototypes.ModifiedBy = PrototypeVersion.ModifiedBy;

                        await _queueService.SendObjectAsync(QueueName.PrototypesFTP, new PrototypesFTPMessage
                        {
                            Company = PrototypeVersion.Prototypes.Company,
                            Prototype = PrototypeVersion.Prototype,
                            PrototypeName = PrototypeVersion.Prototypes.Name,
                            Version = PrototypeVersion.Version,
                            Path = $"{(PrototypeVersion.Prototypes.Companies.Name?.Replace('#', '-') ?? "")}#{(PrototypeVersion.Prototypes.Departments.Name?.Replace('#', '-') ?? "")}#{(PrototypeVersion.Prototypes.PrototypeTypes.Name?.Replace('#', '-') ?? "")}#{(PrototypeVersion.Prototypes.Name?.Replace('#', '-') ?? "")}#{PrototypeVersion.Version}#{(PrototypeVersion.Description?.Replace('#', '-') ?? "")}"
                        });

                        var OldVersion = PrototypeVersion.Prototypes.PrototypeVersions.SingleOrDefault(pv => pv.VersionState == VersionState.Active);
                        if (OldVersion != null)
                        {
                            await SetPrototypeVersionState(OldVersion, VersionState.Obsolete);
                            await _prototypeRepository.SaveChangesAsync();
                        }
                    }

                    await SetPrototypeVersionState(PrototypeVersion, State);
                    await _prototypeRepository.SaveChangesAsync();
                }
                catch (Exception Exception)
                {
                    Logger.Error(Exception);
                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = PrototypeVersion.Prototypes.Company,
                        Action = TrailAction.JobError,
                        User = "(system)",
                        Date = DateTimeOffset.UtcNow,
                        CheckList = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}",
                        Information = JsonConvert.SerializeObject(new
                        {
                            Job = "UpdatePrototypes",
                            Exception = Exception.Message
                        })
                    });

                }
        }
    }
}