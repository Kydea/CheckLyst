using CheckListManager.Common.Logic.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class NotificationDiagnosticsFilter
    {
        public NotificationBucketEventLevel Level { get; set; }
        public string Event { get; set; }
        public long? Company { get; set; }
        public string User { get; set; }
    }
}
