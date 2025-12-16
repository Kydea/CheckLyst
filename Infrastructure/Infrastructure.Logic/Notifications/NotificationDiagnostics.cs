using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class NotificationDiagnostics
    {
        public int GlobalWaiters { get; set; }
        public long TotalWakeups { get; set; }
        public long TotalTimeouts { get; set; }
        public long TotalOverloadReturns { get; set; }
        public long TotalAborts { get; set; }

        public long TotalWaitCompleted { get; set; }
        public double TotalWaitMillisecons { get; set; }
        public double AverageWaitMillisecons { get; set; }

        public DateTime ServerTimeUtc { get; set; }
    }
}
