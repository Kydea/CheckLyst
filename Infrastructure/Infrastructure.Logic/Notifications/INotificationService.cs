using CheckListManager.Common.Core.Services;
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
        NotificationSnapshot GetSnapshot(long company, string user);
        NotificationSnapshot Update(long company, string user, Action<MutableState> mutate);
        Task<NotificationSnapshot> PollingAsync(long company, string user, ulong sinceVersion, CancellationToken cancellationToken);
        NotificationDiagnostics GetDiagnostics();
    }
}
