using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Applications
{
    public class GetStepsRequest : Request
    {
        public long? Prototype { get; set; }
        public int? Version { get; set; }
        public long? Instance { get; set; }
        public long? Report { get; set; }
    }
}
