using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public interface INotificationService : ISingletonService
    {
        Task<NotificationSnapshot> GetSnapshotAsync(long company, string user);
        Task InvalidateAsync(long company, string user);
        Task<NotificationSnapshot> PollingAsync(long company, string user, ulong sinceVersion, CancellationToken cancellationToken);
        NotificationDiagnostics GetDiagnostics();
    }
}
