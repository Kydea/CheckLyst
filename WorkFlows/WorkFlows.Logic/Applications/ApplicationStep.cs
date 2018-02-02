using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Applications
{
    public class ApplicationStep
    {
        public long ID { get; set; }
        public int Step { get; set; }
        public string Reason { get; set; }
        public string Tags { get; set; }
        public string User { get; set; }
        public DateTimeOffset? Date { get; set; }
        public bool? Approved { get; set; }
    }
}
