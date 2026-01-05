using CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators;
using CheckListManager.Common.Logic.Models.Notifications;
using CheckListManager.Infrastructure.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class NotificationService : INotificationService
    {
        private const int MaxWaitersPerUser = 10;
        private const int MaxWaitersGlobal = 10000;
        private static readonly TimeSpan LongPollTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan BucketTtl = TimeSpan.FromHours(1);
        private static readonly TimeSpan BucketIdleTtl = TimeSpan.FromMinutes(5);

        private readonly ConcurrentDictionary<(long Company, string User), UserBucket> _users = new ConcurrentDictionary<(long, string), UserBucket>();

        private int _globalWaiters;

        // counters
        private long _totalWakeups;
        private long _totalTimeouts;
        private long _totalOverloadReturns;
        private long _totalAborts;

        // wait time stats (completed by wakeup or timeout; abort excluded by default)
        private long _waitCompletedTotal;
        private long _waitMillisecondsTotal;

        public async Task InvalidateAsync(long company, string user, Func<NotificationsState, Task<NotificationsState>> loadStateAsync, NotificationInvalidateMask mask)
        {
            if (!_users.TryGetValue((company, user), out var b))
                return;

            EnterBucket(b);
            try
            {
                NotificationsState currentState;
                lock (b.Gate)
                {
                    if (mask.HasFlag(NotificationInvalidateMask.CompletedReportCount)) b.State.CompletedReportCount = null;
                    if (mask.HasFlag(NotificationInvalidateMask.EnqueuedReportCount)) b.State.EnqueuedReportCount = null;
                    if (mask.HasFlag(NotificationInvalidateMask.ReportsToSignCount)) b.State.ReportsToSignCount = null;
                    if (mask.HasFlag(NotificationInvalidateMask.InstancesToSignCount)) b.State.InstancesToSignCount = null;
                    if (mask.HasFlag(NotificationInvalidateMask.PrototypeVersionsToSignCount)) b.State.PrototypeVersionsToSignCount = null;

                    b.LastLoadedUtc = DateTime.MinValue;
                    b.IncrementVersion();

                    if (b.Waiters.Count == 0) return;

                    currentState = b.State.Clone();
                }

                var state = await loadStateAsync(currentState).ConfigureAwait(false);

                List<WaiterEntry> toWake = null;
                NotificationSnapshot snap;

                lock (b.Gate)
                {
                    b.State.CompletedReportCount = state.CompletedReportCount;
                    b.State.EnqueuedReportCount = state.EnqueuedReportCount;
                    b.State.ReportsToSignCount = state.ReportsToSignCount;
                    b.State.InstancesToSignCount = state.InstancesToSignCount;
                    b.State.PrototypeVersionsToSignCount = state.PrototypeVersionsToSignCount;
                    b.LastLoadedUtc = DateTime.UtcNow;
                    b.IncrementVersion();

                    snap = b.ToSnapshot(timedOut: false);
                    snap.Overloaded = false;

                    if (b.Waiters.Count > 0)
                    {
                        toWake = new List<WaiterEntry>(b.Waiters.Values);
                        b.Waiters.Clear();
                    }

                    snap.ActiveWaiters = 0;
                }

                if (toWake != null && toWake.Count > 0)
                {
                    var nowTicks = DateTime.UtcNow.Ticks;
                    var woken = 0;

                    foreach (var w in toWake)
                    {
                        if (w.Tcs.TrySetResult(snap))
                        {
                            woken++;

                            var ms = (nowTicks - w.StartedTicksUtc) / TimeSpan.TicksPerMillisecond;
                            if (ms < 0) ms = 0;

                            Interlocked.Add(ref _waitMillisecondsTotal, ms);
                            Interlocked.Increment(ref _waitCompletedTotal);
                        }
                    }

                    if (woken > 0)
                    {
                        Interlocked.Add(ref _totalWakeups, woken);
                        Interlocked.Add(ref _globalWaiters, -woken);
                    }
                }
            }
            finally
            {
                ExitBucket(company, user, b);
            }
        }

        private async Task EnsureInitializedOrRefreshAsync(UserBucket b, Func<NotificationsState, Task<NotificationsState>> loadStateAsync)
        {
            var nowUtc = DateTime.UtcNow;
            if (b.State.Valid && (nowUtc - b.LastLoadedUtc) < BucketTtl)
                return;

            await b.InitSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                nowUtc = DateTime.UtcNow;
                NotificationsState currentState;
                lock (b.Gate)
                {
                    if (b.State.Valid && (nowUtc - b.LastLoadedUtc) < BucketTtl)
                        return;

                    currentState = b.State.Clone();
                }

                var state = await loadStateAsync(currentState).ConfigureAwait(false);
                lock (b.Gate)
                {
                    b.State.CompletedReportCount = state.CompletedReportCount;
                    b.State.EnqueuedReportCount = state.EnqueuedReportCount;
                    b.State.ReportsToSignCount = state.ReportsToSignCount;
                    b.State.InstancesToSignCount = state.InstancesToSignCount;
                    b.State.PrototypeVersionsToSignCount = state.PrototypeVersionsToSignCount;
                    b.IncrementVersion();
                    b.LastLoadedUtc = nowUtc;
                }
            }
            finally
            {
                b.InitSemaphore.Release();
            }
        }

        public async Task<NotificationSnapshot> PollingAsync(long company, string user, ulong sinceVersion, Func<NotificationsState, Task<NotificationsState>> loadStateAsync, CancellationToken requestAborted)
        {
            var b = GetBucket(company, user);
            EnterBucket(b);
            try
            {
                await EnsureInitializedOrRefreshAsync(b, loadStateAsync).ConfigureAwait(false);

                // Fast-path
                lock (b.Gate)
                {
                    if (sinceVersion == 0 || b.Version != sinceVersion || b.State.Idle)
                    {
                        var s = b.ToSnapshot(timedOut: false);
                        s.Overloaded = false;
                        s.ActiveWaiters = b.Waiters.Count;
                        return s;
                    }
                }

                // Slow-path: waiter registration
                var waiterId = Guid.NewGuid();
                var tcs = new TaskCompletionSource<NotificationSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);

                lock (b.Gate)
                {
                    if (b.Version != sinceVersion)
                    {
                        var s = b.ToSnapshot(timedOut: false);
                        s.Overloaded = false;
                        s.ActiveWaiters = b.Waiters.Count;
                        return s;
                    }

                    // back-pressure
                    if (b.Waiters.Count >= MaxWaitersPerUser || Volatile.Read(ref _globalWaiters) >= MaxWaitersGlobal)
                    {
                        var s = b.ToSnapshot(timedOut: false);
                        s.Overloaded = true;
                        s.ActiveWaiters = b.Waiters.Count;

                        Interlocked.Increment(ref _totalOverloadReturns);
                        return s;
                    }

                    b.Waiters[waiterId] = new WaiterEntry(tcs);
                }

                Interlocked.Increment(ref _globalWaiters);

                // Abort/disconnect
                var abortReg = requestAborted.Register(() =>
                {
                    RemoveWaiter(b, waiterId);

                    if (tcs.TrySetCanceled())
                        Interlocked.Increment(ref _totalAborts);
                });

                // Timeout
                var timeoutCts = new CancellationTokenSource(LongPollTimeout);
                var timeoutReg = timeoutCts.Token.Register(() =>
                {
                    RemoveWaiter(b, waiterId);

                    NotificationSnapshot snap;
                    lock (b.Gate)
                    {
                        snap = b.ToSnapshot(timedOut: true);
                        snap.Overloaded = false;
                        snap.ActiveWaiters = b.Waiters.Count;
                    }

                    if (tcs.TrySetResult(snap))
                    {
                        Interlocked.Increment(ref _totalTimeouts);

                        // timeout = completed wait
                        Interlocked.Add(ref _waitMillisecondsTotal, (long)LongPollTimeout.TotalMilliseconds);
                        Interlocked.Increment(ref _waitCompletedTotal);
                    }
                });

                return await AwaitAndCleanupAsync(tcs.Task, abortReg, timeoutReg, timeoutCts);
            }
            finally
            {
                ExitBucket(company, user, b);
            }
        }

        private static async Task<NotificationSnapshot> AwaitAndCleanupAsync(
            Task<NotificationSnapshot> task,
            CancellationTokenRegistration abortReg,
            CancellationTokenRegistration timeoutReg,
            CancellationTokenSource timeoutCts)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            finally
            {
                abortReg.Dispose();
                timeoutReg.Dispose();
                timeoutCts.Dispose();
            }
        }

        private UserBucket GetBucket(long company, string user) =>
            _users.GetOrAdd((company, user), _ => new UserBucket());


        private void EnterBucket(UserBucket b)
        {
            Interlocked.Increment(ref b.InFlight);
            Interlocked.Exchange(ref b.LastAccessTicks, DateTime.UtcNow.Ticks);
        }

        private void ExitBucket(long company, string user, UserBucket b)
        {
            var left = Interlocked.Decrement(ref b.InFlight);
            if (left == 0)
            {
                bool noWaiters;
                lock (b.Gate) noWaiters = b.Waiters.Count == 0;
                if (!noWaiters) 
                    return;

                var lastAccess = new DateTime(Interlocked.Read(ref b.LastAccessTicks), DateTimeKind.Utc);
                if ((DateTime.UtcNow - lastAccess) < BucketIdleTtl)
                    return;

                if (_users.TryGetValue((company, user), out var current) && ReferenceEquals(current, b))
                    _users.TryRemove((company, user), out _);
            }
        }

        private void RemoveWaiter(UserBucket b, Guid waiterId)
        {
            bool removed;
            lock (b.Gate)
                removed = b.Waiters.Remove(waiterId);

            if (removed)
                Interlocked.Decrement(ref _globalWaiters);
        }

        public NotificationDiagnostics GetDiagnostics()
        {
            var completed = Interlocked.Read(ref _waitCompletedTotal);
            var totalMilliseconds = Interlocked.Read(ref _waitMillisecondsTotal);

            double average = 0;
            if (completed > 0)
                average = (double)totalMilliseconds / completed;

            return new NotificationDiagnostics
            {
                TotalWakeups = Interlocked.Read(ref _totalWakeups),
                TotalTimeouts = Interlocked.Read(ref _totalTimeouts),
                TotalOverloadReturns = Interlocked.Read(ref _totalOverloadReturns),
                TotalAborts = Interlocked.Read(ref _totalAborts),

                GlobalWaiters = Volatile.Read(ref _globalWaiters),

                TotalWaitCompleted = completed,
                TotalWaitMillisecons = totalMilliseconds,
                AverageWaitMillisecons = average,

                ServerTimeUtc = DateTime.UtcNow
            };
        }

        private sealed class UserBucket
        {
            public object Gate { get; } = new object();
            public NotificationsState State { get; } = new NotificationsState();
            public ulong Version { get; private set; } = 1;
            
            public int InFlight;
            public long LastAccessTicks;

            public Dictionary<Guid, WaiterEntry> Waiters { get; } = new Dictionary<Guid, WaiterEntry>();

            public SemaphoreSlim InitSemaphore { get; } = new SemaphoreSlim(1, 1);
            public DateTime LastLoadedUtc { get; set; } = DateTime.MinValue;

            public void IncrementVersion()
            {
                Version++;
                if (Version == 0) Version = 1;
            }

            public NotificationSnapshot ToSnapshot(bool timedOut) => new NotificationSnapshot
            {
                CompletedReportCount = State.CompletedReportCount.Value,
                EnqueuedReportCount = State.EnqueuedReportCount.Value,
                ReportsToSignCount = State.ReportsToSignCount.Value,
                InstancesToSignCount = State.InstancesToSignCount.Value,
                PrototypeVersionsToSignCount = State.PrototypeVersionsToSignCount.Value,
                Idle = State.Idle,
                Version = Version,
                TimedOut = timedOut,
                ServerTimeUtc = DateTime.UtcNow
            };
        }

        private sealed class WaiterEntry
        {
            public TaskCompletionSource<NotificationSnapshot> Tcs { get; }
            public long StartedTicksUtc { get; }

            public WaiterEntry(TaskCompletionSource<NotificationSnapshot> tcs)
            {
                Tcs = tcs ?? throw new ArgumentNullException(nameof(tcs));
                StartedTicksUtc = DateTime.UtcNow.Ticks;
            }
        }
    }
}
