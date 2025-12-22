using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Notifications
{
    public sealed class NotificationsState
    {
        public long CompletedReportCount { get; set; }
        public long EnqueuedReportCount { get; set; }

        public bool Idle => 
            CompletedReportCount == 0
            && EnqueuedReportCount == 0;
    }
}
