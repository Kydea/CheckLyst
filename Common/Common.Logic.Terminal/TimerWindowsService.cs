using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;
using NLog;
using Topshelf;
using Timer = System.Timers.Timer;

namespace CheckListManager.Common.Logic.Terminal
{
	public abstract class TimerWindowsService : WindowsService
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly List<ISchedulerStrategy> _strategies = new List<ISchedulerStrategy>();

		protected TimerWindowsService(TerminalConfig config) : base(config)
		{
		}

        protected void Add(ISchedulerStrategy strategy)
        {
            _strategies.Add(strategy);
        }

		protected override bool OnStart(HostControl hostControl)
		{
            foreach (var strategy in _strategies)
            {
                strategy.ComputeNextExecutionTime();

                strategy.Timer = new Timer
                {
                    AutoReset = true,
                    Interval = strategy.Interval.TotalMilliseconds,
                    Enabled = true
                };

                strategy.Timer.Elapsed += (sender, args) => Run(strategy);

                if (strategy.RunAtStartup)
                    ThreadPool.QueueUserWorkItem(o => Run(strategy));
            }
			return true;
		}

		protected override bool OnStop(HostControl hostControl)
		{
            foreach(var strategy in _strategies)
            {
                strategy.Timer?.Stop();
                strategy.Timer?.Dispose();
                strategy.Timer = null;
            }

			return true;
		}

		private void Run(ISchedulerStrategy strategy)
		{
            if (CancellationTokenSource.IsCancellationRequested)
                return;

            var timer = strategy.Timer;   // snapshot
            if (timer == null)
                return;

            if (strategy.StopTimerWhenRunning)
                timer.Enabled = false;

			try
			{
				var task = strategy.RunHandler(CancellationTokenSource.Token);
                task.Wait();
			}
            catch (OperationCanceledException) { }
            catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OperationCanceledException)) { }
            catch (Exception exception)
			{
				Logger.Error(exception);
			}
			finally
			{
				if (!CancellationTokenSource.IsCancellationRequested && timer != null && strategy.ComputeNextExecutionTime(DateTime.UtcNow)) 
                    timer.Interval = strategy.Interval.TotalMilliseconds;

			    if (!CancellationTokenSource.IsCancellationRequested && timer != null && strategy.StopTimerWhenRunning)
                    timer.Enabled = true;
            }
        }
    }
}
