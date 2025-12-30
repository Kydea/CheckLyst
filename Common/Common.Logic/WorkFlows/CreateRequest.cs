using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.WorkFlows
{
    public class CreateRequest : Request
    {
        public long WorkFlow { get; set; }
        public DateTimeOffset? Date { get; set; }
        public long? Prototype { get; set; }
        public int? Version { get; set; }
        public long? Instance { get; set; }
        public long? Report { get; set; }
        public string Action { get; set; }
    }
}
