using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.SAP
{
    public class SAPParameters
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

        public DateTimeOffset Version { get; set; }
    }
}
