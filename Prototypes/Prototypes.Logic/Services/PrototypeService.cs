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

namespace CheckListManager.Prototypes.Logic.Services
{
    public class PrototypeService : IPrototypeService
    {
        private readonly CheckListJsonConverter _converter;
        private readonly IPrototypesRepository _prototypeRepository;

        public PrototypeService(CheckListJsonConverter converter, IPrototypesRepository prototypeRepository)
        {
            _converter = converter;
            _prototypeRepository = prototypeRepository;
        }

        public async Task<PrototypeResponse> OpenAsync(DefaultIdentity defaultIdentity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(defaultIdentity, request.ID, request.Version);

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
                PrototypeType = PrototypeVersion.Prototypes.PrototypeType
            });
        }

        public async Task<PrototypeResponse> CloneAsync(DefaultIdentity defaultIdentity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(defaultIdentity, request.ID, request.Version);

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

        public async Task<PrototypeResponse> CreateAsync(DefaultIdentity defaultIdentity, PrototypeRequest request)
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

        public async Task<PrototypeResponse> VersionAsync(DefaultIdentity defaultIdentity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(defaultIdentity, request.ID, request.Version);

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

        public async Task<PrototypeResponse> SaveAsync(DefaultIdentity defaultIdentity, PrototypeRequest request)
        {
            request.CheckList.Setup();
            var UnvalidExpression = request.CheckList.Expressions.FirstOrDefault(e => e.Value.Error != null);
            if (UnvalidExpression.Key != null)
                throw new Exception($"The expression '{UnvalidExpression.Key}' contains the error: {UnvalidExpression.Value.Error}");

            if (String.IsNullOrWhiteSpace(request.Name)) throw new Exception("Specify a name");
            if (!request.Department.HasValue) throw new Exception("Specify a department");
            if (!request.PrototypeType.HasValue) throw new Exception("Specify a prototype type");

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
                    Company = defaultIdentity.CompanyId,
                    Department = request.Department.Value,
                    PrototypeType = request.PrototypeType.Value,
                    Description = request.Description,
                    CreatedOn = Now,
                    CreatedBy = defaultIdentity.Code
                };
                _prototypeRepository.AddPrototype(Prototype);
            }
            else
                Prototype = await _prototypeRepository.GetPrototypeAsync(defaultIdentity, request.ID);

            if (NewVersion)
                Prototype.PrototypeVersions.Add(PrototypeVersion = new PrototypeVersions
                {
                    VersionState = VersionState.Draft,
                    Version = Prototype.PrototypeVersions.Count == 0 ? 1 : Prototype.PrototypeVersions.Max(v => v.Version) + 1,
                    CreatedOn = Now,
                    CreatedBy = defaultIdentity.Code
                });
            else
            {
                PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(defaultIdentity, request.ID, request.Version);
                if (PrototypeVersion.VersionState != VersionState.Draft) throw new Exception($"The version {request.ID}.{request.Version} is not editable");
                if (PrototypeVersion.ModifiedOn != request.ModifiedOn || PrototypeVersion.ModifiedBy != request.ModifiedBy) throw new Exception($"Concurrency error: the prototype has already been modified by the user {request.ModifiedOn}");

                Prototype.Name = request.Name;
                Prototype.Description = request.Description;
                Prototype.ModifiedOn = Now;
                Prototype.ModifiedBy = defaultIdentity.Code;

                PrototypeVersion.ModifiedOn = Now;
                PrototypeVersion.ModifiedBy = defaultIdentity.Code;
            }

            PrototypeVersion.Description = request.Description;
            PrototypeVersion.Data = JsonConvert.SerializeObject(request.CheckList, _converter);
            PrototypeVersion.ValidFrom = request.ValidFrom;
            PrototypeVersion.ValidUntil = request.ValidUntil;

            await _prototypeRepository.SaveAsync();

            return new PrototypeResponse
            {
                ID = PrototypeVersion.Prototype,
                Version = PrototypeVersion.Version
            };
        }

        public async Task DeleteAsync(DefaultIdentity defaultIdentity, PrototypeRequest request)
        {
            await _prototypeRepository.DeletePrototypeVersionAsync(defaultIdentity, request.ID, request.Version);
        }

        public async Task<PrototypeResponse> SetStateAsync(DefaultIdentity defaultIdentity, PrototypeRequest request)
        {
            var PrototypeVersion = await _prototypeRepository.SetStateAsync(defaultIdentity, request.ID, request.Version, request.State);
            return new PrototypeResponse
            {
                State = PrototypeVersion.VersionState
            };
        }
    }
}