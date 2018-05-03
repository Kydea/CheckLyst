using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.SAP
{
    public class SAPConfiguration : IDestinationConfiguration
    {
        private static Dictionary<string, SAPParameters> SourceParametersDictionary { get; } = new Dictionary<string, SAPParameters>();

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public static void Check(string Code, SAPParameters SAPParameters, DateTimeOffset Version)
        {
            if (!SourceParametersDictionary.ContainsKey(Code) || Version - SourceParametersDictionary[Code].Version > TimeSpan.Zero)
                SourceParametersDictionary.Add(Code, SAPParameters);
        }

        public SAPConfiguration()
        {
        }

        public bool ChangeEventsSupported()
        {
            return false;
        }

        public RfcConfigParameters GetParameters(string Code)
        {
            var ConfigParameters = new RfcConfigParameters();
            var SAPParameters = SourceParametersDictionary[Code];
            ConfigParameters.Add(RfcConfigParameters.Name, Code);
            ConfigParameters.Add(RfcConfigParameters.Client, SAPParameters.Client);
            ConfigParameters.Add(RfcConfigParameters.MessageServerHost, SAPParameters.MessageServerHost);
            ConfigParameters.Add(RfcConfigParameters.MessageServerService, SAPParameters.MessageServerService);
            ConfigParameters.Add(RfcConfigParameters.SystemID, SAPParameters.SystemID);
            ConfigParameters.Add(RfcConfigParameters.LogonGroup, SAPParameters.LogonGroup);
            ConfigParameters.Add(RfcConfigParameters.User, SAPParameters.User);
            ConfigParameters.Add(RfcConfigParameters.Password, SAPParameters.Password);
            ConfigParameters.Add(RfcConfigParameters.Language, SAPParameters.Language);
            ConfigurationChanged?.Invoke(Code, new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, ConfigParameters));
            return ConfigParameters;
        }
    }
}
