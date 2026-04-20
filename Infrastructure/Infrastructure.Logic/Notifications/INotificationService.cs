using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
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
        Task InvalidateAsync(long company, string user, Func<NotificationsState, Task<NotificationsState>> loadStateAsync, NotificationInvalidateMask mask);
        Task<NotificationSnapshot> PollingAsync(long company, string user, ulong sinceVersion, Func<NotificationsState, Task<NotificationsState>> loadStateAsync, CancellationToken cancellationToken);
        NotificationDiagnostics GetDiagnostics(PagingRequest<NotificationDiagnosticsFilter> request);
    }
}
