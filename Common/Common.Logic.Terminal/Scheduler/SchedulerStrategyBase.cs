using System;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public abstract class SchedulerStrategyBase : ISchedulerStrategy
	{
        public SchedulerStrategyBase(Func<System.Threading.CancellationToken, Task> RunHandler)
        {
            this.RunHandler = RunHandler;
        }

        public abstract TimeSpan Interval { get; }

		public virtual bool StopTimerWhenRunning { get; set; } = true;
		public virtual bool RunAtStartup { get; set; } = true;

		public virtual bool ComputeNextExecutionTime(DateTime? start = null)
		{
			return false;
		}

        public Func<System.Threading.CancellationToken, Task> RunHandler { get; }

        public System.Timers.Timer Timer { get; set; }
    }
}
