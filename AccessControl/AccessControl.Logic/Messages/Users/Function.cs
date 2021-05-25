using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
    public class Grant
    {
        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
        public string FunctionGroup { get; set; }
        public string Role { get; set; }
        public string PrototypeWorkFlowLevel { get; set; }
        public string InstanceWorkFlowLevel { get; set; }
        public string SignatureLevel { get; set; }
        public string Function { get; set; }
        public string[] Functions { get; set; }
    }
}
