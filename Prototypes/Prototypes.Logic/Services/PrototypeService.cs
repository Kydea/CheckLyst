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
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.Services;
using System.Collections.Generic;
using System.IO;
using CheckListManager.Common.Logic.Models.CheckLists;
using System.Threading;
using CheckListManager.Common.Logic.Api.Instances;
using CheckListManager.Common.Logic.Email;
using NLog;
using CheckListManager.Common.Logic.FTP;
using CheckListManager.Common.Logic.Models;

namespace CheckListManager.Prototypes.Logic.Services
{
    public class PrototypeService : IPrototypeService
    {
        private readonly CheckListJsonConverter _converter;
        private readonly IPrototypesRepository _prototypeRepository;
        private readonly IQueueService _queueService;
        private readonly ICommonRepository _commonRepository;
        private readonly IStorageService _storageService;
        private readonly IConfigurationService _configurationService;
        private readonly IInstancesService _instancesService;
        private readonly IEmailServiceBase _emailService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PrototypeService(
            CheckListJsonConverter converter,
            IPrototypesRepository prototypeRepository,
            IQueueService queueService,
            ICommonRepository commonRepository,
            IStorageService storageService,
            IConfigurationService configurationService,
            IInstancesService instancesService,
            IEmailServiceBase emailService
        )
        {
            _converter = converter;
            _prototypeRepository = prototypeRepository;
            _queueService = queueService;
            _commonRepository = commonRepository;
            _storageService = storageService;
            _configurationService = configurationService;
            _instancesService = instancesService;
            _emailService = emailService;
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
            Prototype.Setup(false);

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
            MarkImagesAsCloned(Prototype);
            Prototype.Setup(false);

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
                Header = new Group { Description = "(header)" },
                Valid = true
            };

            Page Page;
            Prototype.Pages.Add(Page = new Page { Description = "(page)" });
            Page.Groups.Add(new Group { Description = "(group)" });

            return await Task.FromResult(new PrototypeResponse
            {
                ID = -1,
                Version = -1,
                CheckList = Prototype,
                State = VersionState.Draft
            });
        }

        public async Task CheckNewVersionAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            await _prototypeRepository.CheckNewVersionAsync(identity.CompanyId, request.ID);
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
            MarkImagesAsCloned(Prototype);
            Prototype.Setup(false);

            return await Task.FromResult(new PrototypeResponse
            {
                ID = PrototypeVersion.Prototype,
                Version = -1,
                CheckList = Prototype,
                Name = PrototypeVersion.Prototypes.Name,
                Description = PrototypeVersion.Description,
                PrototypeType = PrototypeVersion.Prototypes.PrototypeType,
                PrototypeTypeName = PrototypeVersion.Prototypes.PrototypeTypes.Name,
                Department = PrototypeVersion.Prototypes.Department,
                DepartmentName = PrototypeVersion.Prototypes.Departments.Name,
                InstanceConfirmationWorkFlow = PrototypeVersion.InstanceConfirmationWorkFlow,

                State = VersionState.Draft
            });
        }

        public PrototypeResponse Verify(PrototypeRequest request)
        {
            request.CheckList.Setup(true);

            return new PrototypeResponse
            {
                CheckList = request.CheckList
            };

        }

        private void AddTags(Dictionary<string, PrototypeVersionTags> Tags, PrototypeRequest request, ICheckListGroup Group)
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

        private List<PrototypeVersionTags> CreateTags(PrototypeRequest request)
        {
            var Tags = new Dictionary<string, PrototypeVersionTags>();
            AddTags(Tags, request, request.CheckList.Header);
            return Tags.Values.ToList();
        }

        public async Task<PrototypeResponse> SaveAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            if (
                request.Department.HasValue && !identity.HasPermission("Prototypes", "Edit", request.Department.Value) &&
                request.PrototypeType.HasValue && !identity.HasPermission("Prototypes", "Edit", request.Department.Value, request.PrototypeType.Value)
            )
                throw new UnauthorizedAccessException();

            request.CheckList.Errors.Clear();
            request.CheckList.Setup(false);
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                request.CheckList.Errors.Add(new Models.CheckLists.Configuration.Error("name", "Specify a name"));
                request.CheckList.Valid = false;
            }
            if (!request.Department.HasValue)
            {
                request.CheckList.Errors.Add(new Models.CheckLists.Configuration.Error("department", "Specify a department"));
                request.CheckList.Valid = false;
            }
            if (!request.PrototypeType.HasValue)
            {
                request.CheckList.Errors.Add(new Models.CheckLists.Configuration.Error("prototype-type", "Specify a prototype type"));
                request.CheckList.Valid = false;
            }

            if (!request.CheckList.Valid)
                return new PrototypeResponse
                {
                    CheckList = request.CheckList
                };

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
                if (
                    (PrototypeVersion.ModifiedOn?.UtcDateTime.Ticks ?? 0) - (request.ModifiedOn?.UtcDateTime.Ticks ?? 0) > 10000 /*ignored under milliseconds*/
                    || PrototypeVersion.ModifiedBy != request.ModifiedBy
                ) throw new Exception($"Concurrency error: the prototype has already been modified by the user {request.ModifiedBy}"); //the db approximate some tick, we take 10 as maximum

                PrototypeVersion.ModifiedOn = Now;
                PrototypeVersion.ModifiedBy = identity.Code;
            }

            //Get the new id
            if (NewPrototype || NewVersion) await _prototypeRepository.SaveChangesAsync();

            await CloneImagesAsync(
                request.CheckList,
                identity.CompanyId,
                PrototypeVersion.Prototype,
                PrototypeVersion.Version
            );

            //Processes the deleted images attachment
            foreach (var key in request.ImagesToDelete)
                await DeleteAttachmentAsync(identity.CompanyId, PrototypeVersion.Prototype, PrototypeVersion.Version, key);

            PrototypeVersion.Description = request.Description;
            PrototypeVersion.Data = JsonConvert.SerializeObject(request.CheckList, _converter);
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
                ModifiedOn = PrototypeVersion.ModifiedOn,
                CheckList = request.CheckList
            };
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
            if (PrototypeVersion.VersionState == VersionState.InSimulation && (request.State == VersionState.Approved && request.StateProposed || request.State == VersionState.Draft))
            {
                if (
                    !identity.HasPermission("Prototypes", "Preview", PrototypeVersion.Prototypes.Department) &&
                    !identity.HasPermission("Prototypes", "Preview", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
                )
                    throw new UnauthorizedAccessException();
            }
            else if (
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            var instances = PrototypeVersion.Instances.ToList();
            var setStateResult = await _prototypeRepository.SetStateAsync(identity, PrototypeVersion, request.State, request.StateProposed);
            if (setStateResult.HasInstancesDeleted)
                await DeleteAttachmentFromInstancesAsync(identity, instances);
            if (setStateResult.HasToInvalidateCache)
                await _instancesService.InvalidateCacheAsync(new InvalidateCacheRequest(
                    PrototypeVersion.Prototype,
                    PrototypeVersion.Version,
                    instances.Select(i => i.Code).ToList()
                ));

            if (request.StateProposed && request.State == VersionState.Approved) //we track both the state change and the proposed state for the workflow
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

        private async Task DeleteAttachmentFromInstancesAsync(IDefaultIdentity identity, List<Instances> instances)
        {
            foreach (var instance in instances)
            {
                await _storageService.DeleteRecursiveAsync(
                    _configurationService.InstancesStorageBucket,
                    $"{identity.CompanyId}/{instance.Code}/"
                );
            }
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
                    User = s.User == null ? string.Empty : $"{s.User} - {s.UserDescription}",
                    Level = $"({string.Join(", ", s.WorkFlowApplicationStepTags.Select(t => t.Tag))})",
                    Reason = s.Reason,
                    Comment = s.Comment,
                    Approved = s.Approved,
                    AppointedUser = s.User == null ? string.Join(", ", _commonRepository.GetAppointedUsers(identity.CompanyId, "Prototypes", "Approve", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType, s.WorkFlowApplicationStepTags.Select(t => t.Tag)).Select(u => u.Code)) : null
                }).ToList()
            }).ToList());
        }

        private async Task SetPrototypeVersionState(PrototypeVersions PrototypeVersion, string State, DateTimeOffset Date)
        {
            PrototypeVersion.VersionState = State;
            if (State == VersionState.Obsolete && PrototypeVersion.ValidUntil == null)
                PrototypeVersion.ValidUntil = Date;

            PrototypeVersion.PrototypeVersionsHistory.Add(new PrototypeVersionsHistory
            {
                Prototype = PrototypeVersion.Prototype,
                Version = PrototypeVersion.Version,
                Date = Date,
                State = State,
                User = "(system)"
            });

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = PrototypeVersion.Prototypes.Company,
                Action = TrailAction.PrototypeStateAutoChanged,
                User = "(system)",
                Date = Date,
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

                        var OldVersion = PrototypeVersion.Prototypes.PrototypeVersions.SingleOrDefault(pv => pv.VersionState == VersionState.Active);
                        if (OldVersion != null)
                        {
                            await SetPrototypeVersionState(OldVersion, VersionState.Obsolete, UtcNow);
                            await _prototypeRepository.SaveChangesAsync();
                        }
                    }

                    await _queueService.SendObjectAsync(QueueName.PrototypesFTP, new PrototypesFTPMessage
                    {
                        Company = PrototypeVersion.Prototypes.Company,
                        Prototype = PrototypeVersion.Prototype,
                        PrototypeName = PrototypeVersion.Prototypes.Name,
                        Version = PrototypeVersion.Version,
                        State = State,
                        Path = $"{(PrototypeVersion.Prototypes.Companies.Name?.Replace('#', '-') ?? "")}#{(PrototypeVersion.Prototypes.Departments.Name?.Replace('#', '-') ?? "")}#{(PrototypeVersion.Prototypes.PrototypeTypes.Name?.Replace('#', '-') ?? "")}#{(PrototypeVersion.Prototypes.Name?.Replace('#', '-') ?? "")}#{PrototypeVersion.Version}#{(PrototypeVersion.Description?.Replace('#', '-') ?? "")}"
                    }, CancellationToken);

                    await SetPrototypeVersionState(PrototypeVersion, State, UtcNow);
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
                    }, CancellationToken);
                    await _emailService.LogErrorAsync(Exception, "Error updating prototype version state");
                }
        }

        #region Attachment

        private async Task ValidateRequest(
            DefaultIdentity identity,
            AttachmentRequestBase request,
            bool isEdit = true
        )
        {
            var prototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity.CompanyId, request.Id, request.Version);
            var department = prototypeVersion.Prototypes.Department;
            var prototypeType = prototypeVersion.Prototypes.PrototypeType;

            var function = isEdit ? "Edit" : "View";

            if (
                !identity.HasPermission("Prototypes", function, department) &&
                !identity.HasPermission("Prototypes", function, department, prototypeType) &&
                !identity.HasPermission("Instances", function, department) &&
                !identity.HasPermission("Instances", function, department, prototypeType)
            )
                throw new UnauthorizedAccessException();

            if (isEdit && prototypeVersion.VersionState != VersionState.Draft)
                throw new Exception("The prototype version is not in state draft");
        }

        public async Task<UploadAttachmentResponse> UploadAttachmentAsync(DefaultIdentity identity, UploadAttachmentRequest request)
        {
            await ValidateRequest(identity, request);
            var extension = Path.GetExtension(request.Attachment.FileName);

            var imageName = request.Code ?? Guid.NewGuid().ToString();
            var key = $"{identity.CompanyId}/{request.Id}-{request.Version}/{imageName}{extension}";

            using (var memoryStream = new MemoryStream(request.Attachment.Buffer))
            {
                var success = await _storageService.PutAsync(_configurationService.PrototypesStorageBucket, key, memoryStream);
                if (!success)
                    throw new Exception("Error uploading the attachment");
            }

            return new UploadAttachmentResponse { Key = key, ImageName = imageName };
        }

        public async Task<StorageFile> DownloadAttachmentAsync(DefaultIdentity identity, ProcessAttachmentRequest request)
        {
            await ValidateRequest(identity, request, false);

            var startIndex = request.Key.IndexOf('/');
            if (startIndex < 0)
                throw new InvalidOperationException("Key format non valid");
            var key = $"{identity.CompanyId}{request.Key.Substring(startIndex)}";

            return await _storageService.GetAsync(_configurationService.PrototypesStorageBucket, key);
        }

        public async Task DeleteAttachmentAsync(long company, long prototype, int version, string key)
        {
            if (key != null && key.StartsWith($"{company}/{prototype}-{version}/"))
                try
                {
                    await _storageService.DeleteAsync(_configurationService.PrototypesStorageBucket, key);
                }
                catch
                {
                    //the deletion of the images can fail for example if the upload of that image failed
                }
        }

        #endregion

        private void MarkImagesAsCloned(Prototype prototype)
        {
            prototype.Traverse(
                item => item is Image image && image.Key != null,
                item =>
                {
                    var image = (Image)item;
                    image.Cloned = true;
                }
            );
        }

        private async Task CloneImagesAsync(Prototype prototype, long companyId, long prototypeId, int version)
        {
            await prototype.TraverseAsync(
                item => item is Image image && image.Cloned,
                async item =>
                {
                    var image = (Image)item;
                    if (image.Key != null)
                    {
                        image.ImageName = Guid.NewGuid().ToString();
                        var Key = $"{companyId}/{prototypeId}-{version}/{image.ImageName}{Path.GetExtension(image.Key)}";

                        if (!await _storageService.CloneAsync(_configurationService.PrototypesStorageBucket, image.Key, Key))
                            throw new Exception($"Could not clone the file of the image {image.Id}-{image.Description ?? "(no label)"} from {image.Key} to {Key}");

                        image.Key = Key;
                    }
                    image.Cloned = false;
                }
            );
        }
    }
}