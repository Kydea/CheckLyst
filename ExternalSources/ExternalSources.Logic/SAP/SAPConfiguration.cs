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
            var Parameters = new RfcConfigParameters();
            var SAPParameters = SourceParametersDictionary[Code];
            Parameters.Add(RfcConfigParameters.Client, SAPParameters.Client);
            Parameters.Add(RfcConfigParameters.MessageServerHost, SAPParameters.MessageServerHost);
            Parameters.Add(RfcConfigParameters.MessageServerService, SAPParameters.MessageServerService);
            Parameters.Add(RfcConfigParameters.SystemID, SAPParameters.SystemID);
            Parameters.Add(RfcConfigParameters.LogonGroup, SAPParameters.LogonGroup);
            Parameters.Add(RfcConfigParameters.User, SAPParameters.User);
            Parameters.Add(RfcConfigParameters.Password, SAPParameters.Password);
            Parameters.Add(RfcConfigParameters.Language, SAPParameters.Language);
            return Parameters;
        }
    }
}
