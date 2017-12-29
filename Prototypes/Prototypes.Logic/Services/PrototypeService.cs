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

namespace CheckListManager.Prototypes.Logic.Services
{
    public class PrototypeService : IPrototypeService
    {
        private readonly CheckListJsonConverter _converter;
        private readonly IPrototypesRepository _prototypeRepository;
        private readonly IQueueService _queueService;

        public PrototypeService(CheckListJsonConverter converter, IPrototypesRepository prototypeRepository, IQueueService queueService)
        {
            _converter = converter;
            _prototypeRepository = prototypeRepository;
            _queueService = queueService;
        }

        public async Task<PrototypeResponse> OpenAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity, request.ID, request.Version);
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
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity, request.ID, request.Version);
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
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity, request.ID, request.Version);
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
                        Tags[Item.Description] = (new PrototypeVersionTags
                        {
                            Prototype = request.ID,
                            Version = request.Version,
                            Code = Item.Description
                        });
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
                Prototype.PrototypeVersions.Add(PrototypeVersion = new PrototypeVersions
                {
                    VersionState = VersionState.Draft,
                    Version = Prototype.PrototypeVersions.Count == 0 ? 1 : Prototype.PrototypeVersions.Max(v => v.Version) + 1,
                    CreatedOn = Now,
                    CreatedBy = identity.Code
                });
            else
            {
                PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity, request.ID, request.Version);
                if (PrototypeVersion.VersionState != VersionState.Draft) throw new Exception($"The version {request.ID}.{request.Version} is not editable");
                if ((PrototypeVersion.ModifiedOn?.UtcDateTime.Ticks ?? 0) - (request.ModifiedOn?.UtcDateTime.Ticks ?? 0) > 10 || PrototypeVersion.ModifiedBy != request.ModifiedBy) throw new Exception($"Concurrency error: the prototype has already been modified by the user {request.ModifiedBy}"); //the db approximate some tick

                Prototype.Name = request.Name;
                Prototype.Description = request.Description;
                Prototype.ModifiedOn = Now;
                Prototype.ModifiedBy = identity.Code;

                PrototypeVersion.ModifiedOn = Now;
                PrototypeVersion.ModifiedBy = identity.Code;
            }

            PrototypeVersion.Description = request.Description;
            PrototypeVersion.Data = JsonConvert.SerializeObject(request.CheckList, _converter);
            PrototypeVersion.ValidFrom = request.ValidFrom;
            PrototypeVersion.ValidUntil = request.ValidUntil;

            //Deletes the old tags and creates the new ones
            PrototypeVersion.PrototypeVersionTags.ToList().ForEach(t => PrototypeVersion.PrototypeVersionTags.Remove(t));
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

        public async Task DeleteAsync(DefaultIdentity identity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity, request.ID, request.Version);
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
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(identity, request.ID, request.Version);
            if (
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department) &&
                !identity.HasPermission("Prototypes", "Edit", PrototypeVersion.Prototypes.Department, PrototypeVersion.Prototypes.PrototypeType)
            )
                throw new UnauthorizedAccessException();

            await _prototypeRepository.SetStateAsync(identity, PrototypeVersion, request.State, request.StateProposed);

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
    }
}