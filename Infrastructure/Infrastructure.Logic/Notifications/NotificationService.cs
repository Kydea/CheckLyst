using CheckListManager.Common.Logic.Models.Notifications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Notifications
{
    public sealed class NotificationService : INotificationService
    {
        private const int MaxWaitersPerUser = 10;
        private const int MaxWaitersGlobal = 10000;
        private const int MaxHistoryRows = 50000;

        private static readonly TimeSpan LongPollTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan BucketTtl = TimeSpan.FromHours(1);
        private static readonly TimeSpan BucketIdleTtl = TimeSpan.FromMinutes(5);

        private readonly ConcurrentDictionary<(long Company, string User), UserBucket> _users = new ConcurrentDictionary<(long, string), UserBucket>();
        private readonly ConcurrentQueue<NotificationBucketEvent> _history = new ConcurrentQueue<NotificationBucketEvent>();

        private int _globalWaiters;
        private long _historySequence;

        // counters
        private long _totalWakeups;
        private long _totalTimeouts;
        private long _totalOverloadReturns;
        private long _totalAborts;

        // wait time stats (completed by wakeup or timeout; abort excluded by default)
        private long _waitCompletedTotal;
        private long _waitMillisecondsTotal;

        public async Task InvalidateAsync
        (
            long company,
            string user,
            Func<NotificationsState, Task<NotificationsState>> loadStateAsync,
            NotificationInvalidateMask mask
        )
        {
            if (!_users.TryGetValue((company, user), out var b))
            {
                AppendHistory
                (
                    NotificationBucketEventLevel.Info,
                    "InvalidateMiss",
                    "invalidate",
                    company,
                    user,
                    mask: mask
                );

                return;
            }

            AppendHistory
            (
                NotificationBucketEventLevel.Info,
                "InvalidateRequested",
                "invalidate",
                b,
                mask: mask
            );

            EnterBucket(b);
            try
            {
                await RefreshBucketAsync(
                    b,
                    loadStateAsync,
                    forceRefresh: true,
                    invalidateMask: mask
                ).ConfigureAwait(false);
            }
            finally
            {
                ExitBucket(b);
            }
        }

        private async Task EnsureInitializedOrRefreshAsync
        (
            UserBucket b,
            Func<NotificationsState, Task<NotificationsState>> loadStateAsync
        )
        {
            lock (b.Gate)
            {
                if (IsFresh_NoLock(b, DateTime.UtcNow))
                    return;
            }

            await RefreshBucketAsync(
                b,
                loadStateAsync,
                forceRefresh: false,
                invalidateMask: null
            ).ConfigureAwait(false);
        }

        private async Task RefreshBucketAsync
        (
            UserBucket b,
            Func<NotificationsState, Task<NotificationsState>> loadStateAsync,
            bool forceRefresh,
            NotificationInvalidateMask? invalidateMask
        )
        {
            await b.InitSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                NotificationsState requestState;

                lock (b.Gate)
                {
                    if (!forceRefresh && IsFresh_NoLock(b, DateTime.UtcNow))
                        return;

                    requestState = b.State.Clone();

                    if (invalidateMask.HasValue)
                        ApplyInvalidateMask(requestState, invalidateMask.Value);
                }

                AppendHistory
                (
                    NotificationBucketEventLevel.Debug,
                    "RefreshStart",
                    invalidateMask.HasValue ? "invalidate" : "polling",
                    b,
                    mask: invalidateMask,
                    note: forceRefresh ? "forced" : "normal"
                );

                var loadedState = await loadStateAsync(requestState).ConfigureAwait(false);

                List<WaiterEntry> toWake = null;
                NotificationSnapshot snap = null;
                var woken = 0;

                lock (b.Gate)
                {
                    ApplyLoadedState(b.State, loadedState);

                    b.LastLoadedUtc = DateTime.UtcNow;
                    b.IncrementVersion();

                    if (b.Waiters.Count > 0)
                    {
                        snap = b.ToSnapshot(timedOut: false);
                        snap.Overloaded = false;

                        toWake = new List<WaiterEntry>(b.Waiters.Values);
                        woken = toWake.Count;
                        b.Waiters.Clear();

                        snap.ActiveWaiters = 0;
                    }
                }

                AppendHistory
                (
                    NotificationBucketEventLevel.Debug,
                    "RefreshEnd",
                    invalidateMask.HasValue ? "invalidate" : "polling",
                    b,
                    mask: invalidateMask
                );

                WakeWaiters(b, toWake, snap, woken);
            }
            finally
            {
                b.InitSemaphore.Release();
            }
        }

        private static bool IsFresh_NoLock(UserBucket b, DateTime nowUtc) => b.State.Valid && (nowUtc - b.LastLoadedUtc) < BucketTtl;

        private static void ApplyInvalidateMask
        (
            NotificationsState state,
            NotificationInvalidateMask mask
        )
        {
            if (mask.HasFlag(NotificationInvalidateMask.CompletedReportCount))
                state.CompletedReportCount = null;

            if (mask.HasFlag(NotificationInvalidateMask.EnqueuedReportCount))
                state.EnqueuedReportCount = null;

            if (mask.HasFlag(NotificationInvalidateMask.ReportsToSignCount))
                state.ReportsToSignCount = null;

            if (mask.HasFlag(NotificationInvalidateMask.InstancesToSignCount))
                state.InstancesToSignCount = null;

            if (mask.HasFlag(NotificationInvalidateMask.PrototypeVersionsToSignCount))
                state.PrototypeVersionsToSignCount = null;
        }

        private static void ApplyLoadedState
        (
            NotificationsState target,
            NotificationsState source
        )
        {
            target.CompletedReportCount = source.CompletedReportCount;
            target.EnqueuedReportCount = source.EnqueuedReportCount;
            target.ReportsToSignCount = source.ReportsToSignCount;
            target.InstancesToSignCount = source.InstancesToSignCount;
            target.PrototypeVersionsToSignCount = source.PrototypeVersionsToSignCount;
        }

        private void WakeWaiters
        (
            UserBucket b,
            List<WaiterEntry> toWake,
            NotificationSnapshot snap,
            int expectedWoken
        )
        {
            if (toWake == null || toWake.Count == 0)
                return;

            var nowTicks = DateTime.UtcNow.Ticks;
            var woken = 0;

            foreach (var w in toWake)
                if (w.Tcs.TrySetResult(snap))
                {
                    woken++;

                    var ms = (nowTicks - w.StartedTicksUtc) / TimeSpan.TicksPerMillisecond;
                    if (ms < 0) ms = 0;

                    Interlocked.Add(ref _waitMillisecondsTotal, ms);
                    Interlocked.Increment(ref _waitCompletedTotal);
                }

            if (woken > 0)
            {
                Interlocked.Add(ref _totalWakeups, woken);
                Interlocked.Add(ref _globalWaiters, -woken);

                AppendHistory
                (
                    NotificationBucketEventLevel.Debug,
                    "WaitersWoken",
                    "system",
                    b,
                    note: $"Woken={woken}; Expected={expectedWoken}"
                );
            }
        }

        public async Task<NotificationSnapshot> PollingAsync
        (
            long company,
            string user,
            ulong sinceVersion,
            Func<NotificationsState, Task<NotificationsState>> loadStateAsync,
            CancellationToken requestAborted
        )
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

                        AppendHistory
                        (
                            NotificationBucketEventLevel.Warning,
                            "OverloadReturned",
                            "polling",
                            b,
                            sinceVersion
                        );

                        return s;
                    }

                    b.Waiters[waiterId] = new WaiterEntry(tcs);
                }

                Interlocked.Increment(ref _globalWaiters);

                AppendHistory
                (
                    NotificationBucketEventLevel.Debug,
                    "WaiterRegistered",
                    "polling",
                    b,
                    sinceVersion
                );

                // Abort/disconnect
                var abortReg = requestAborted.Register(() =>
                {
                    RemoveWaiter(b, waiterId);

                    if (tcs.TrySetCanceled())
                    {
                        Interlocked.Increment(ref _totalAborts);

                        AppendHistory
                        (
                            NotificationBucketEventLevel.Warning,
                            "WaiterAborted",
                            "polling",
                            b,
                            sinceVersion
                        );
                    }
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

                        AppendHistory
                        (
                            NotificationBucketEventLevel.Warning,
                            "WaiterTimeout",
                            "polling",
                            b,
                            sinceVersion
                        );
                    }
                });

                return await AwaitAndCleanupAsync(tcs.Task, abortReg, timeoutReg, timeoutCts);
            }
            finally
            {
                ExitBucket(b);
            }
        }

        private static async Task<NotificationSnapshot> AwaitAndCleanupAsync
        (
            Task<NotificationSnapshot> task,
            CancellationTokenRegistration abortReg,
            CancellationTokenRegistration timeoutReg,
            CancellationTokenSource timeoutCts
        )
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

        private UserBucket GetBucket(long company, string user)
        {
            var key = (company, user);

            if (_users.TryGetValue(key, out var existing))
                return existing;

            var candidate = new UserBucket(company, user);
            var bucket = _users.GetOrAdd(key, candidate);

            if (ReferenceEquals(bucket, candidate))
                AppendHistory(NotificationBucketEventLevel.Debug, "BucketCreated", "system", bucket);

            return bucket;
        }

        private void EnterBucket(UserBucket b)
        {
            Interlocked.Increment(ref b.InFlight);
            Interlocked.Exchange(ref b.LastAccessTicks, DateTime.UtcNow.Ticks);
        }

        private void ExitBucket(UserBucket b)
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

                if (_users.TryGetValue((b.Company, b.User), out var current) && ReferenceEquals(current, b))
                {
                    AppendHistory
                    (
                        NotificationBucketEventLevel.Info,
                        "BucketEvicted",
                        "system",
                        b
                    );

                    _users.TryRemove((b.Company, b.User), out _);
                }
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

                ServerTimeUtc = DateTime.UtcNow,
                History = _history.ToList()
            };
        }

        private void AppendHistory
        (
            NotificationBucketEventLevel level,
            string eventType,
            string source,
            UserBucket b,
            ulong? sinceVersion = null,
            NotificationInvalidateMask? mask = null,
            string note = null
        )
        {
            NotificationBucketEvent e;

            lock (b.Gate)
                e = new NotificationBucketEvent
                {
                    Sequence = Interlocked.Increment(ref _historySequence),
                    TimestampUtc = DateTime.UtcNow,

                    Level = level,
                    EventType = eventType,
                    Source = source,

                    Company = b.Company,
                    User = b.User,

                    SinceVersion = sinceVersion,
                    Version = b.Version,
                    Mask = mask.HasValue ? (int)mask.Value : (int?)null,

                    CompletedReportCount = b.State.CompletedReportCount,
                    EnqueuedReportCount = b.State.EnqueuedReportCount,
                    ReportsToSignCount = b.State.ReportsToSignCount,
                    InstancesToSignCount = b.State.InstancesToSignCount,
                    PrototypeVersionsToSignCount = b.State.PrototypeVersionsToSignCount,

                    ActiveWaiters = b.Waiters.Count,
                    GlobalWaiters = Volatile.Read(ref _globalWaiters),
                    InFlight = Volatile.Read(ref b.InFlight),

                    LastLoadedUtc = b.LastLoadedUtc,
                    Note = note
                };

            _history.Enqueue(e);

            while (_history.Count > MaxHistoryRows && _history.TryDequeue(out _)) ;
        }

        private void AppendHistory
        (
            NotificationBucketEventLevel level,
            string eventType,
            string source,
            long company,
            string user,
            ulong? sinceVersion = null,
            NotificationInvalidateMask? mask = null,
            string note = null
        )
        {
            _history.Enqueue(new NotificationBucketEvent
            {
                Sequence = Interlocked.Increment(ref _historySequence),
                TimestampUtc = DateTime.UtcNow,

                Level = level,
                EventType = eventType,
                Source = source,

                Company = company,
                User = user,

                SinceVersion = sinceVersion,
                Version = 0,
                Mask = mask.HasValue ? (int)mask.Value : (int?)null,
                Note = note
            });

            while (_history.Count > MaxHistoryRows && _history.TryDequeue(out _)) ;
        }

        private sealed class UserBucket
        {
            public UserBucket(long company, string user)
            {
                Company = company;
                User = user;
            }

            public long Company { get; }
            public string User { get; }

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
