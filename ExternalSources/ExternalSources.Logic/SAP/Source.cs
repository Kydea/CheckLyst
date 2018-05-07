using Newtonsoft.Json;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.SAP
{
    public class Source : SourceBase
    {
        private Object Lock = new System.Object();

        private static SAPConfiguration SAPConfiguration { get; set; }

        private RfcDestination Destination { get; set; }
        private SAPParameters SAPParameters { get;set;}

        public override void Parse(string Data)
        {
            SAPParameters = JsonConvert.DeserializeObject<SAPParameters>(Data);
            BuildParameters();
            BuildColumns();
        }

        public override void Open()
        {
            lock (Lock)
            {
                if (SAPConfiguration == null)
                    RfcDestinationManager.RegisterDestinationConfiguration(SAPConfiguration = new SAPConfiguration());
                SAPConfiguration.Check(Code, SAPParameters, Version, Password);
            }

            Destination = RfcDestinationManager.GetDestination(Code);
        }

        public override void Dispose()
        {
        }

        private void BuildParameters()
        {
            switch (SAPParameters.FunctionName)
            {
                case "PO Header":
                    Parameters = new List<Parameter> { new Parameter { Label = "PO Number", Name = "PO", ParameterType = "Text" } };
                    break;
                default:
                    throw new NotSupportedException($"Source name {SAPParameters.FunctionName}");
            }
        }

        private void BuildColumns()
        {
            switch (SAPParameters.FunctionName)
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
                    throw new NotSupportedException($"Source name {SAPParameters.FunctionName}");
            }
        }

        public override void Test()
        {
            var Repository = Destination.Repository;
        }

        public override IEnumerable<List<string>> GetData(Dictionary<string, string> Parameters, int Page)
        {
            for (int Index = 0; Index < Page; ++Index)
                yield return new List<string> { $"{Index}", "Colonna 2", "Colonna 3" };
        }
    }
}
