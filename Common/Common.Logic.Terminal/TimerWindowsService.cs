using System;
using System.Threading.Tasks;
using System.Timers;
using CheckListManager.Common.Logic.Terminal.Configuration;
using NLog;
using Topshelf;

namespace CheckListManager.Common.Logic.Terminal
{
	public abstract class TimerWindowsService : WindowsService
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly int _interval;

		protected Timer Timer { get; private set; }

		protected virtual bool StopTimerWhenRunning { get; } = false;

		protected TimerWindowsService(TerminalConfig config, int interval) : base(config)
		{
			_interval = interval;
		}

		protected override bool OnStart(HostControl hostControl)
		{
			Timer = new Timer
			{
				AutoReset = true,
				Interval = _interval,
				Enabled = true
			};

			Timer.Elapsed += (sender, args) => Run();

			return true;
		}

		protected override bool OnStop(HostControl hostControl)
		{
			Timer.Stop();

			return true;
		}

		private void Run()
		{
			if (StopTimerWhenRunning)
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

			if (StopTimerWhenRunning)
			{
				Timer.Enabled = true;
			}
		}

		protected abstract Task RunAsync();
	}
}
