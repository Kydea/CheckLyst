using System;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public abstract class SchedulerStrategyBase : ISchedulerStrategy
	{
		public abstract TimeSpan Interval { get; }

		public virtual bool StopTimerWhenRunning { get; set; } = true;
		public virtual bool RunAtStartup { get; set; } = true;

		public virtual bool ComputeNextExecutionTime(DateTime? start = null)
		{
			return false;
		}
	}
}
