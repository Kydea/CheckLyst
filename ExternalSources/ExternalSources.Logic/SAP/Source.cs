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

        private static bool ConfigurationRegistered { get; set; }
        private RfcDestination Destination { get; set; }
        private SAPParameters SourceParameters { get;set;}

        public override void Parse(string Data)
        {
            SourceParameters = JsonConvert.DeserializeObject<SAPParameters>(Data);
            BuildParameters();
            BuildColumns();
        }

        public override void Open()
        {
            lock (Lock)
            {
                if (!ConfigurationRegistered)
                {
                    RfcDestinationManager.RegisterDestinationConfiguration(new SAPConfiguration());
                    ConfigurationRegistered = true;
                }

                SAPConfiguration.Check(Code, SourceParameters, Version);
            }

            Destination = RfcDestinationManager.GetDestination(Code);
        }

        public override void Dispose()
        {
        }

        private void BuildParameters()
        {
            switch (SourceParameters.FunctionName)
            {
                case "PO Header":
                    Parameters = new List<Parameter> { new Parameter { Label = "PO Number", Name = "PO", ParameterType = "Text" } };
                    break;
                default:
                    throw new NotSupportedException($"Source name {SourceParameters.FunctionName}");
            }
        }

        private void BuildColumns()
        {
            switch (SourceParameters.FunctionName)
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
                    throw new NotSupportedException($"Source name {SourceParameters.FunctionName}");
            }
        }

        public override void Test()
        {
            var Repository = Destination.Repository;
        }
    }
}
