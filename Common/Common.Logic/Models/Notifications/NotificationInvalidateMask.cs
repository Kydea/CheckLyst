using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Notifications
{
    [Flags]
    public enum NotificationInvalidateMask
    {
        None = 0,
        CompletedReportCount = 1 << 0,
        EnqueuedReportCount = 1 << 1,
        ReportsToSignCount = 1 << 2,
        InstancesToSignCount = 1 << 3,
        PrototypeVersionsToSignCount = 1 << 4,
        All = CompletedReportCount | EnqueuedReportCount | ReportsToSignCount | InstancesToSignCount | PrototypeVersionsToSignCount
    }
}
