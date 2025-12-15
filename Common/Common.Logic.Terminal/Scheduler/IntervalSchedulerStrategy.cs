using System;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public class IntervalSchedulerStrategy : SchedulerStrategyBase
	{
		public IntervalSchedulerStrategy(Func<System.Threading.CancellationToken, Task> RunHandler, int interval) : base(RunHandler)
		{
			Interval = TimeSpan.FromSeconds(interval);
		}

		public override TimeSpan Interval { get; }
	}
}
