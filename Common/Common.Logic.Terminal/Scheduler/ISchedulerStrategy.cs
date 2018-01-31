using System;
using System.Threading.Tasks;
using System.Timers;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public interface ISchedulerStrategy
	{
		TimeSpan Interval { get; }

		bool StopTimerWhenRunning { get; set; }
		bool RunAtStartup { get; set; }

		bool ComputeNextExecutionTime(DateTime? start = null);

        Func<Task> RunHandler { get; }

        Timer Timer { get; set; }
    }
}
