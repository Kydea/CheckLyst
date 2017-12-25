using System;
using System.Threading;
using System.Threading.Tasks;
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

		private readonly ISchedulerStrategy _strategy;

		protected Timer Timer { get; private set; }

		protected TimerWindowsService(TerminalConfig config, ISchedulerStrategy strategy) : base(config)
		{
			_strategy = strategy;
		}

		protected override bool OnStart(HostControl hostControl)
		{
			_strategy.ComputeNextExecutionTime();

			Timer = new Timer
			{
				AutoReset = true,
				Interval = _strategy.Interval.TotalMilliseconds,
				Enabled = true
			};

			Timer.Elapsed += (sender, args) => Run();

			if (_strategy.RunAtStartup)
			{
				ThreadPool.QueueUserWorkItem(o => Run());
			}

			return true;
		}

		protected override bool OnStop(HostControl hostControl)
		{
			Timer.Stop();

			return true;
		}

		private void Run()
		{
			if (_strategy.StopTimerWhenRunning)
			{
				Timer.Enabled = false;
			}

			try
			{
				RunAsync().Wait(CancellationTokenSource.Token);
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}
			finally
			{
				if (_strategy.ComputeNextExecutionTime(DateTime.UtcNow))
				{
					Timer.Interval = _strategy.Interval.TotalMilliseconds;
				}
			}

			if (_strategy.StopTimerWhenRunning)
			{
				Timer.Enabled = true;
			}
		}

		protected abstract Task RunAsync();
	}
}
