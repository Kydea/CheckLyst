using System;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public interface ISchedulerStrategy
	{
		TimeSpan Interval { get; }

		bool StopTimerWhenRunning { get; set; }
		bool RunAtStartup { get; set; }

		bool ComputeNextExecutionTime(DateTime? start = null);

        Func<System.Threading.CancellationToken, Task> RunHandler { get; }

        System.Timers.Timer Timer { get; set; }
    }
}
