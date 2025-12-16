using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class NotificationService : INotificationService
    {
        private static long TicksToMs(long ticks) => ticks / TimeSpan.TicksPerMillisecond;

        private const int MaxWaitersPerUser = 10;
        private const int MaxWaitersGlobal = 10000;
        private static readonly TimeSpan LongPollTimeout = TimeSpan.FromSeconds(30);

        private readonly ConcurrentDictionary<string, UserBucket> _users =
            new ConcurrentDictionary<string, UserBucket>(StringComparer.Ordinal);

        private int _globalWaiters;

        // counters
        private long _totalWakeups;
        private long _totalTimeouts;
        private long _totalOverloadReturns;
        private long _totalAborts;

        // wait time stats (completed by wakeup or timeout; abort excluded by default)
        private long _waitCompletedTotal;
        private long _waitMillisTotal;

        public NotificationSnapshot GetSnapshot(long company, string user)
        {
            var b = GetBucket(company, user);
            lock (b.Gate)
            {
                var s = b.ToSnapshot(timedOut: false);
                s.Overloaded = false;
                s.ActiveWaiters = b.Waiters.Count;
                return s;
            }
        }

        public NotificationSnapshot Update(long company, string user, Action<MutableState> mutate)
        {
            var b = GetBucket(company, user);

            List<WaiterEntry> toRelease = null;
            int releasedCount = 0;

            NotificationSnapshot snap;
            lock (b.Gate)
            {
                mutate(b.State);

                if (b.State.TaskCount < 0) b.State.TaskCount = 0;
                if (b.State.ReportCount < 0) b.State.ReportCount = 0;

                b.Version++;
                if (b.Version == 0) b.Version = 1;

                snap = b.ToSnapshot(timedOut: false);

                if (b.Waiters.Count > 0)
                {
                    releasedCount = b.Waiters.Count;
                    toRelease = new List<WaiterEntry>(b.Waiters.Values);
                    b.Waiters.Clear();
                }
            }

            if (releasedCount > 0)
                Interlocked.Add(ref _globalWaiters, -releasedCount);

            if (toRelease != null)
            {
                Interlocked.Increment(ref _totalWakeups);

                var nowTicks = DateTime.UtcNow.Ticks;

                foreach (var entry in toRelease)
                {
                    // latency metric
                    var waitedMs = TicksToMs(nowTicks - entry.StartedTicksUtc);
                    if (waitedMs < 0) waitedMs = 0;

                    Interlocked.Add(ref _waitMillisTotal, waitedMs);
                    Interlocked.Increment(ref _waitCompletedTotal);

                    entry.Tcs.TrySetResult(snap);
                }
            }

            return snap;
        }

        public Task<NotificationSnapshot> PollingAsync(long company, string user, ulong sinceVersion, CancellationToken requestAborted)
        {
            var b = GetBucket(company, user);

            // Fast-path
            lock (b.Gate)
            {
                if (sinceVersion == 0 || b.Version != sinceVersion)
                {
                    var s = b.ToSnapshot(timedOut: false);
                    s.Overloaded = false;
                    s.ActiveWaiters = b.Waiters.Count;
                    return Task.FromResult(s);
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
                    return Task.FromResult(s);
                }

                // back-pressure
                if (b.Waiters.Count >= MaxWaitersPerUser || Volatile.Read(ref _globalWaiters) >= MaxWaitersGlobal)
                {
                    var s = b.ToSnapshot(timedOut: false);
                    s.Overloaded = true;
                    s.ActiveWaiters = b.Waiters.Count;

                    Interlocked.Increment(ref _totalOverloadReturns);
                    return Task.FromResult(s);
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
                    Interlocked.Add(ref _waitMillisTotal, (long)LongPollTimeout.TotalMilliseconds);
                    Interlocked.Increment(ref _waitCompletedTotal);
                }
            });

            return AwaitAndCleanupAsync(tcs.Task, abortReg, timeoutReg, timeoutCts);
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
            _users.GetOrAdd(company + "|" + user, _ => new UserBucket());

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
            var totalMilliseconds = Interlocked.Read(ref _waitMillisTotal);

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
            public MutableState State { get; } = new MutableState();
            public ulong Version { get; set; } = 1;

            public Dictionary<Guid, WaiterEntry> Waiters { get; } = new Dictionary<Guid, WaiterEntry>();

            public NotificationSnapshot ToSnapshot(bool timedOut) => new NotificationSnapshot
            {
                TaskCount = State.TaskCount,
                ReportCount = State.ReportCount,
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
