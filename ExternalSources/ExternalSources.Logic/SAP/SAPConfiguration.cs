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

        public void Check(string Code, SAPParameters SAPParameters, DateTimeOffset Version, string Password)
        {
            if (!SourceParametersDictionary.ContainsKey(Code) || Version - SourceParametersDictionary[Code].Version > TimeSpan.Zero)
            {
                SAPParameters.Version = Version;
                SAPParameters.Password = Password;
                SourceParametersDictionary[Code] = SAPParameters;
                ConfigurationChanged?.Invoke(Code, new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, GetParameters(Code)));
            }
        }

        public bool ChangeEventsSupported()
        {
            return true;
        }

        public RfcConfigParameters GetParameters(string Code)
        {
            var SAPParameters = SourceParametersDictionary[Code];
            var Parameters = new RfcConfigParameters
            {
                { RfcConfigParameters.Client, SAPParameters.Client },
                { RfcConfigParameters.MessageServerHost, SAPParameters.MessageServerHost },
                { RfcConfigParameters.MessageServerService, SAPParameters.MessageServerService },
                { RfcConfigParameters.SystemID, SAPParameters.SystemID },
                { RfcConfigParameters.LogonGroup, SAPParameters.LogonGroup },
                { RfcConfigParameters.User, SAPParameters.User },
                { RfcConfigParameters.Password, SAPParameters.Password },
                { RfcConfigParameters.Language, SAPParameters.Language }
            };
            return Parameters;
        }
    }
}
