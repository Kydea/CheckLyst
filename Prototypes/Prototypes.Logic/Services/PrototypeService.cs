using System.Threading.Tasks;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;

namespace CheckListManager.Prototypes.Logic.Services
{
    public class PrototypeService : IPrototypeService
    {
        private readonly CheckListJsonConverter _converter;

        public PrototypeService(CheckListJsonConverter converter)
        {
            _converter = converter;
        }

        public Task<GetPrototypeResponse> OpenAsync(long companyId, GetPrototypeRequest request)
        {
            const string data = "{\r\n\t\"Pages\": [{\r\n\t\t\"Groups\": [{\r\n\t\t\t\"TypeName\": \"Group\",\r\n\t\t\t\"CSSClass\": \"\",\r\n\t\t\t\"Items\": [],\r\n\t\t\t\"Iterations\": [],\r\n\t\t\t\"Iterator\": false,\r\n\t\t\t\"InIterator\": false,\r\n\t\t\t\"Description\": \"(Group)\",\r\n\t\t\t\"NewLine\": false,\r\n\t\t\t\"Id\": 0,\r\n\t\t\t\"RuntimeValues\": {},\r\n\t\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\t\"ValidExpressionCode\": null,\r\n\t\t\t\"ValidCondition\": 0,\r\n\t\t\t\"ValidityMessage\": null,\r\n\t\t\t\"Code\": null,\r\n\t\t\t\"SignatureCode\": null\r\n\t\t}],\r\n\t\t\"Description\": \"(Page)\",\r\n\t\t\"Lock\": null,\r\n\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\"ValidExpressionCode\": null,\r\n\t\t\"ValidCondition\": 0,\r\n\t\t\"ValidityMessage\": null,\r\n\t\t\"Code\": null,\r\n\t\t\"SignatureCode\": null\r\n\t}],\r\n\t\"Expressions\": {},\r\n\t\"Sources\": {},\r\n\t\"Header\": {\r\n\t\t\"TypeName\": \"Group\",\r\n\t\t\"CSSClass\": \"\",\r\n\t\t\"Items\": [{\r\n\t\t\t\"TypeName\": \"Field\",\r\n\t\t\t\"LinesCount\": 0,\r\n\t\t\t\"ValueTypeName\": \"Text\",\r\n\t\t\t\"ValueFormulaString\": null,\r\n\t\t\t\"ValuesList\": null,\r\n\t\t\t\"ExternalSourceName\": null,\r\n\t\t\t\"ExternalSourceColumn\": null,\r\n\t\t\t\"Value\": null,\r\n\t\t\t\"InIterator\": false,\r\n\t\t\t\"Description\": \"label\",\r\n\t\t\t\"NewLine\": false,\r\n\t\t\t\"CSSClass\": \"Field\",\r\n\t\t\t\"Id\": 0,\r\n\t\t\t\"RuntimeValues\": {},\r\n\t\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\t\"ValidExpressionCode\": null,\r\n\t\t\t\"ValidCondition\": 0,\r\n\t\t\t\"ValidityMessage\": null,\r\n\t\t\t\"Code\": \"lb0\",\r\n\t\t\t\"SignatureCode\": null\r\n\t\t}],\r\n\t\t\"Iterations\": [],\r\n\t\t\"Iterator\": false,\r\n\t\t\"InIterator\": false,\r\n\t\t\"Description\": \"(Header)\",\r\n\t\t\"NewLine\": false,\r\n\t\t\"Id\": 0,\r\n\t\t\"RuntimeValues\": {},\r\n\t\t\"ReadOnlyExpressionCode\": null,\r\n\t\t\"ReadOnlyCondition\": 0,\r\n\t\t\"HiddenIfReadOnly\": false,\r\n\t\t\"ValidExpressionCode\": null,\r\n\t\t\"ValidCondition\": 0,\r\n\t\t\"ValidityMessage\": null,\r\n\t\t\"Code\": null,\r\n\t\t\"SignatureCode\": null\r\n\t},\r\n\t\"Code\": \"text\",\r\n\t\"Version\": 1,\r\n\t\"Company\": \"Janssen\",\r\n\t\"Department\": \"Maintenance\",\r\n\t\"PrototypeType\": \"Reconciliation\",\r\n\t\"Description\": \"desc\",\r\n\t\"InstanceDescription\": null,\r\n\t\"ValidFrom\": \"2016-04-21T00:00:00\",\r\n\t\"ValidUntil\": null,\r\n\t\"State\": \"In approval\",\r\n\t\"Note\": \"def\"\r\n}";
            var prototype = JsonConvert.DeserializeObject<Prototype>(data, _converter);

            return Task.FromResult(new GetPrototypeResponse
            {
                Id = request.Id,
                Version = request.Version,
                CheckList = prototype
            });
        }

        public Task<GetPrototypeResponse> CloneAsync(long companyId, GetPrototypeRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetPrototypeResponse> CreateAsync(long companyId, GetPrototypeRequest request)
        {
            var Prototype = new Prototype
            {
                Header = new Group { Description = "(Header)" },
                State = "Draft"
            };

            Page Page;
            Prototype.Pages.Add(Page = new Page { Description = "(Page)" });
            Page.Groups.Add(new Group { Description = "(Group)" });

            return Task.FromResult(new GetPrototypeResponse
            {
                Id = request.Id,
                Version = request.Version,
                //    Version = -1,
                //    ValidFrom = null,
                //    ValidUntil = null,

                CheckList = Prototype
            });
        }

        public Task<GetPrototypeResponse> VersionAsync(long companyId, GetPrototypeRequest request)
        {
            throw new NotImplementedException();
        }
    }
}