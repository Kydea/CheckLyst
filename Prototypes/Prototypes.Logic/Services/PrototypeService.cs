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

        public async Task<PrototypeResponse> OpenAsync(long companyId, PrototypeRequest request)
        {
            const string data = "{\r\n\t\"Pages\": [{\r\n\t\t\"Groups\": [{\r\n\t\t\t\"TypeName\": \"Group\",\r\n\t\t\t\"CSSClass\": \"\",\r\n\t\t\t\"Items\": [],\r\n\t\t\t\"Iterations\": [],\r\n\t\t\t\"Iterator\": false,\r\n\t\t\t\"InIterator\": false,\r\n\t\t\t\"Description\": \"(Group)\",\r\n\t\t\t\"NewLine\": false,\r\n\t\t\t\"Id\": 0,\r\n\t\t\t\"RuntimeValues\": {},\r\n\t\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\t\"ValidExpressionCode\": null,\r\n\t\t\t\"ValidCondition\": 0,\r\n\t\t\t\"ValidityMessage\": null,\r\n\t\t\t\"Code\": null,\r\n\t\t\t\"SignatureCode\": null\r\n\t\t}],\r\n\t\t\"Description\": \"(Page)\",\r\n\t\t\"Lock\": null,\r\n\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\"ValidExpressionCode\": null,\r\n\t\t\"ValidCondition\": 0,\r\n\t\t\"ValidityMessage\": null,\r\n\t\t\"Code\": null,\r\n\t\t\"SignatureCode\": null\r\n\t}],\r\n\t\"Expressions\": {},\r\n\t\"Sources\": {},\r\n\t\"Header\": {\r\n\t\t\"TypeName\": \"Group\",\r\n\t\t\"CSSClass\": \"\",\r\n\t\t\"Items\": [{\r\n\t\t\t\"TypeName\": \"Field\",\r\n\t\t\t\"LinesCount\": 0,\r\n\t\t\t\"ValueTypeName\": \"Text\",\r\n\t\t\t\"ValueFormulaString\": null,\r\n\t\t\t\"ValuesList\": null,\r\n\t\t\t\"ExternalSourceName\": null,\r\n\t\t\t\"ExternalSourceColumn\": null,\r\n\t\t\t\"Value\": null,\r\n\t\t\t\"InIterator\": false,\r\n\t\t\t\"Description\": \"label\",\r\n\t\t\t\"NewLine\": false,\r\n\t\t\t\"CSSClass\": \"Field\",\r\n\t\t\t\"Id\": 0,\r\n\t\t\t\"RuntimeValues\": {},\r\n\t\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\t\"ValidExpressionCode\": null,\r\n\t\t\t\"ValidCondition\": 0,\r\n\t\t\t\"ValidityMessage\": null,\r\n\t\t\t\"Code\": \"lb0\",\r\n\t\t\t\"SignatureCode\": null\r\n\t\t}],\r\n\t\t\"Iterations\": [],\r\n\t\t\"Iterator\": false,\r\n\t\t\"InIterator\": false,\r\n\t\t\"Description\": \"(Header)\",\r\n\t\t\"NewLine\": false,\r\n\t\t\"Id\": 0,\r\n\t\t\"RuntimeValues\": {},\r\n\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\"ValidExpressionCode\": null,\r\n\t\t\"ValidCondition\": 0,\r\n\t\t\"ValidityMessage\": null,\r\n\t\t\"Code\": null,\r\n\t\t\"SignatureCode\": null\r\n\t},\r\n\t\"Code\": \"text\",\r\n\t\"Version\": 1,\r\n\t\"Company\": \"Janssen\",\r\n\t\"Department\": \"Maintenance\",\r\n\t\"PrototypeType\": \"Reconciliation\",\r\n\t\"Description\": \"desc\",\r\n\t\"InstanceDescription\": null,\r\n\t\"ValidFrom\": \"2016-04-21T00:00:00\",\r\n\t\"ValidUntil\": null,\r\n\t\"State\": \"In approval\",\r\n\t\"Note\": \"def\"\r\n}";
            var prototype = JsonConvert.DeserializeObject<Prototype>(data, _converter);

            return await Task.FromResult(new PrototypeResponse
            {
                Id = request.ID,
                Version = request.Version,
                PrototypeType = 1,
                CheckList = prototype,
                ModifiedOn = null, //TODO
                ModifiedBy = null //TODO
            });
        }

        public Task<PrototypeResponse> CloneAsync(long companyId, PrototypeRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<PrototypeResponse> CreateAsync(long companyId, PrototypeRequest request)
        {
            //expression and items are only for test purpose
            var BooleanExpression = new Common.Logic.Models.Expressions.Expression { SerializedValue = "true" };
            var DateTimeExpression = new Common.Logic.Models.Expressions.Expression { SerializedValue = "Signature.SignedOn" };
            var Prototype = new Prototype
            {
                Header = new Group { Description = "(header)" },
                State = "Draft",
                ValidFrom = null,
                ValidUntil = DateTimeOffset.UtcNow.AddDays(10),
                Expressions = new System.Collections.Generic.Dictionary<string, Common.Logic.Models.Expressions.Expression> {
                    { "BooleanExpression", BooleanExpression },
                    { "DateTimeExpression", DateTimeExpression }
                }
            };

            Page Page;
            Group Group;
            Prototype.Pages.Add(Page = new Page { Description = "(page)" });
            Page.Groups.Add(Group = new Group { Description = "(group)" });
            Group.Items.Add(new Signature { Code = "Signature" });

            Prototype.Setup();

            return await Task.FromResult(new PrototypeResponse
            {
                Id = -1,
                Version = -1,

                CheckList = Prototype
            });
        }

        public Task<PrototypeResponse> VersionAsync(long companyId, PrototypeRequest request)
        {
            throw new NotImplementedException();
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

            if (String.IsNullOrWhiteSpace(request.CheckList.Name)) throw new Exception("Specify a name");
            if (String.IsNullOrWhiteSpace(request.CheckList.Description)) throw new Exception("Specify a description");
            if (!request.CheckList.Department.HasValue) throw new Exception("Specify a department");
            if (!request.CheckList.PrototypeType.HasValue) throw new Exception("Specify a prototype type");

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
                    Name = request.CheckList.Name,
                    Company = defaultIdentity.CompanyId,
                    Department = request.CheckList.Department.Value,
                    PrototypeType = request.CheckList.PrototypeType.Value,
                    Description = request.CheckList.Description,
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
                    Description = request.CheckList.Description,
                    VersionState = VersionState.Draft,
                    Version = Prototype.PrototypeVersions.Count == 0 ? 1 : Prototype.PrototypeVersions.Max(v => v.Version) + 1,
                    Data = JsonConvert.SerializeObject(request.CheckList, _converter),
                    CreatedOn = Now,
                    CreatedBy = defaultIdentity.Code
                });
            else
            {
                PrototypeVersion = await _prototypeRepository.GetPrototypeVersionAsync(defaultIdentity, request.ID, request.Version);
                if (PrototypeVersion.VersionState != VersionState.Draft) throw new Exception($"The version {request.ID}.{request.Version} is not editable");
                if (PrototypeVersion.ModifiedOn != request.ModifiedOn || PrototypeVersion.ModifiedBy != request.ModifiedBy) throw new Exception($"Concurrency error: the prototype has already been modified by the user {request.ModifiedOn}");

                Prototype.Name = request.CheckList.Name;
                Prototype.Description = request.CheckList.Description;
                Prototype.ModifiedOn = Now;
                Prototype.ModifiedBy = defaultIdentity.Code;
                PrototypeVersion.Description = request.CheckList.Description;
                PrototypeVersion.Data = JsonConvert.SerializeObject(request.CheckList, _converter);
                PrototypeVersion.ValidFrom = request.CheckList.ValidFrom;
                PrototypeVersion.ValidUntil = request.CheckList.ValidUntil;

                PrototypeVersion.ModifiedOn = Now;
                PrototypeVersion.ModifiedBy = defaultIdentity.Code;
            }

            await _prototypeRepository.SaveAsync();

            return new PrototypeResponse
            {
                Id = PrototypeVersion.Prototype,
                Version = PrototypeVersion.Version
            };
        }
    }
}