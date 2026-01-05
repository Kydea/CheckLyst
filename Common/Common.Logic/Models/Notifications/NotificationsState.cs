using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Notifications
{
    public sealed class NotificationsState
    {
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

        public NotificationsState Clone() => new NotificationsState
        {
            CompletedReportCount = CompletedReportCount,
            EnqueuedReportCount = EnqueuedReportCount,
            ReportsToSignCount = ReportsToSignCount,
            InstancesToSignCount = InstancesToSignCount,
            PrototypeVersionsToSignCount = PrototypeVersionsToSignCount
        };
    }
}
