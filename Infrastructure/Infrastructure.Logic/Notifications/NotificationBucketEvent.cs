using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class NotificationBucketEvent
    {
        public long Sequence { get; set; }
        public DateTime TimestampUtc { get; set; }

        public NotificationBucketEventLevel Level { get; set; }
        public string EventType { get; set; }
        public string Source { get; set; } // polling / invalidate / system

        public long Company { get; set; }
        public string User { get; set; }

        public ulong? SinceVersion { get; set; }
        public ulong Version { get; set; }

        public int? Mask { get; set; }

        public long? CompletedReportCount { get; set; }
        public long? EnqueuedReportCount { get; set; }
        public long? ReportsToSignCount { get; set; }
        public long? InstancesToSignCount { get; set; }
        public long? PrototypeVersionsToSignCount { get; set; }

        public bool Idle =>
            CompletedReportCount == 0
            && EnqueuedReportCount == 0
            && ReportsToSignCount == 0
            && InstancesToSignCount == 0
            && PrototypeVersionsToSignCount == 0;

        public bool Valid =>
            CompletedReportCount.HasValue
            && EnqueuedReportCount.HasValue
            && ReportsToSignCount.HasValue
            && InstancesToSignCount.HasValue
            && PrototypeVersionsToSignCount.HasValue;

        public int ActiveWaiters { get; set; }
        public int GlobalWaiters { get; set; }
        public int InFlight { get; set; }

        public DateTime LastLoadedUtc { get; set; }

        public string Note { get; set; }
    }
}
