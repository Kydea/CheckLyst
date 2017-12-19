using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Applications
{
    public class GetStepsResponse
    {
        public List<ApplicationStep> Steps { get; set; }
        public string Action { get; set; }
    }
}
