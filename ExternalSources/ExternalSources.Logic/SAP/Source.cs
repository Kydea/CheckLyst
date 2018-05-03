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
        private RfcDestination destination { get; set; }
        private SAPParameters SourceParameters { get; } = new SAPParameters();

        public override void Parse(string Data)
        {
            var SourceParameters = JsonConvert.DeserializeObject<SAPParameters>(Data);

            this.SourceParameters.FunctionName = SourceParameters.FunctionName;
            this.SourceParameters.Client = SourceParameters.Client;
            this.SourceParameters.MessageServerHost = SourceParameters.MessageServerHost;
            this.SourceParameters.MessageServerService = SourceParameters.MessageServerService;
            this.SourceParameters.SystemID = SourceParameters.SystemID;
            this.SourceParameters.LogonGroup = SourceParameters.LogonGroup;
            this.SourceParameters.User = SourceParameters.User;
            this.SourceParameters.Language = SourceParameters.Language;

            BuildParameters();
            BuildColumns();
        }

        public override void Open()
        {
            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            destination = RfcDestinationManager.GetDestination("RAE");
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
            var Repository = destination.Repository;
        }

        public string Password { get { return SourceParameters.Password; } set { SourceParameters.Password = value; } }

        private class SAPParameters
        {
            public string FunctionName { get; set; }
            public string Client { get; set; }
            public string MessageServerHost { get; set; }
            public string MessageServerService { get; set; }
            public string SystemID { get; set; }
            public string LogonGroup { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
            public string Language { get; set; }
        }

        private class Configuration : IDestinationConfiguration
        {
            //private SAPParameters Parameters { get; }
            public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

            public Configuration()
            {
            }

            public bool ChangeEventsSupported()
            {
                return false;
            }

            public RfcConfigParameters GetParameters(string destinationName)
            {
                var ConfigParameters = new RfcConfigParameters();
                ConfigParameters.Add(RfcConfigParameters.Client, "050");
                ConfigParameters.Add(RfcConfigParameters.MessageServerHost, "dbcirae.jnj.com");
                ConfigParameters.Add(RfcConfigParameters.MessageServerService, "3601");
                ConfigParameters.Add(RfcConfigParameters.SystemID, "RAE");
                ConfigParameters.Add(RfcConfigParameters.LogonGroup, "COMMON");
                ConfigParameters.Add(RfcConfigParameters.User, "CPICITLABPI"); //DBL_PPRLATDW //CPICITLABPI //APEZZOLI
                ConfigParameters.Add(RfcConfigParameters.Password, "lat_web0"); //ITS_pw2014 //lat_web0 //Alessia,2
                ConfigParameters.Add(RfcConfigParameters.Language, "EN");
                //ConfigParameters.Add(RfcConfigParameters.Client, Parameters.Client);
                //ConfigParameters.Add(RfcConfigParameters.MessageServerHost, Parameters.MessageServerHost);
                //ConfigParameters.Add(RfcConfigParameters.MessageServerService, Parameters.MessageServerService);
                //ConfigParameters.Add(RfcConfigParameters.SystemID, Parameters.SystemID);
                //ConfigParameters.Add(RfcConfigParameters.LogonGroup, Parameters.LogonGroup);
                //ConfigParameters.Add(RfcConfigParameters.User, Parameters.User);
                //ConfigParameters.Add(RfcConfigParameters.Password, Parameters.Password);
                //ConfigParameters.Add(RfcConfigParameters.Language, Parameters.Language);
                ConfigurationChanged?.Invoke(destinationName, new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, ConfigParameters));
                return ConfigParameters;
            }
        }
    }
}
