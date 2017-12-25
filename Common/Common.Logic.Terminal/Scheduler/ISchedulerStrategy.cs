using System;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public interface ISchedulerStrategy
	{
		TimeSpan Interval { get; }

		bool StopTimerWhenRunning { get; set; }
		bool RunAtStartup { get; set; }

		bool ComputeNextExecutionTime(DateTime? start = null);
	}
}
