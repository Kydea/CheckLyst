using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Data.Sources
{
    public class SAP : SourceBase
    {
        public override void Parse(string Data)
        {
            var SAPSource = JsonConvert.DeserializeObject<SAP>(Data);

            functionName = SAPSource.functionName;

            Client = SAPSource.Client;
            MessageServerHost = SAPSource.MessageServerHost;
            MessageServerService = SAPSource.MessageServerService;
            SystemID = SAPSource.SystemID;
            LogonGroup = SAPSource.LogonGroup;
            User = SAPSource.User;
            Password = SAPSource.Password;
            Language = SAPSource.Language;

            BuildParameters();
            BuildColumns();
        }

        private void BuildParameters()
        {
            switch (functionName)
            {
                case "PO Header":
                    Parameters = new List<Parameter> { new Parameter { Label = "PO Number", Name = "PO", ParameterType = "Text" } };
                    break;
                default:
                    throw new NotSupportedException($"Source name {functionName}");
            }
        }

        private void BuildColumns()
        {
            switch (functionName)
            {
                case "PO Header":
                    Columns = new List<string>
                    {
                        "PO",
                        "Type",
                        "Line",
                        "Operation",
                        "Operation Description",
                        "Material Number",
                        "Material Description",
                        "Batch",
                        "Scheduled Quantity",
                        "Effective Quantity",
                        "DHT ?",
                        "PO End Date ?",
                        "Equipment"
                    };
                    break;
                default:
                    throw new NotSupportedException($"Source name {functionName}");
            }
        }

        public string functionName { get; set; }

        public string Client { get; set; }
        public string MessageServerHost { get; set; }
        public string MessageServerService { get; set; }
        public string SystemID { get; set; }
        public string LogonGroup { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Language { get; set; }
    }
}
