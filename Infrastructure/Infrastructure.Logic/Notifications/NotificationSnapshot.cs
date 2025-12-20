using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Infrastructure.Data.ListItems;
using CheckListManager.Infrastructure.Logic.PrototypeTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class NotificationSnapshot
    {
        public long ReportCount { get; set; }
        public ulong Version { get; set; }
        public bool TimedOut { get; set; }
        public DateTime ServerTimeUtc { get; set; }

        public bool Overloaded { get; set; } //wait per back-pressure
        public int ActiveWaiters { get; set; } //diagnostic
    }
}
