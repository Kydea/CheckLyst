using System;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public class IntervalSchedulerStrategy : SchedulerStrategyBase
	{
		public IntervalSchedulerStrategy(int interval)
		{
			Interval = TimeSpan.FromSeconds(interval);
		}

		public override TimeSpan Interval { get; }
	}
}
