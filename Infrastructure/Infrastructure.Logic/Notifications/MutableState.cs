using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class MutableState
    {
        public long TaskCount { get; set; }
        public long ReportCount { get; set; }
    }
}
