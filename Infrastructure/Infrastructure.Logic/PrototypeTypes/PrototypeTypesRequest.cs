using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.PrototypeTypes
{
    public class PrototypeTypesRequest : Request
    {
        public long? ID { get; set; }
        public long? Department { get; set; }
        public string Name { get; set; }
        public string FunctionGroup { get; set; }
        public string Function { get; set; }
        public long? ApprovationWorkFlow { get; set; }
        public long? ObsolescenceWorkFlow { get; set; }
        public long? PrototypesFTPServer { get; set; }
        public long? InstancesFTPServer { get; set; }

        public string Reason { get; set; }
    }
}
