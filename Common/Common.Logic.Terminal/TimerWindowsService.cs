using System;
using System.Collections.Generic;
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
			    strategy.Timer.Stop();

			return true;
		}

		private void Run(ISchedulerStrategy strategy)
		{
			if (strategy.StopTimerWhenRunning)
                strategy.Timer.Enabled = false;

			try
			{
                strategy.RunHandler().Wait(CancellationTokenSource.Token);
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}
			finally
			{
				if (strategy.ComputeNextExecutionTime(DateTime.UtcNow))
                    strategy.Timer.Interval = strategy.Interval.TotalMilliseconds;
			}

			if (strategy.StopTimerWhenRunning)
                strategy.Timer.Enabled = true;
		}
	}
}
