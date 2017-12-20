using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Applications
{
    public class SignRequest : Request
    {
        public long ApplicationStep { get; set; }
        public string Password { get; set; }
        public string Comment { get; set; }
        public bool Approved { get; set; }
        public bool ValidFromNow { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public bool ValidUntilNow { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
    }
}
