using System;
using System.Threading.Tasks;
using System.Timers;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public abstract class SchedulerStrategyBase : ISchedulerStrategy
	{
        private Func<Task> runHandler;

        public SchedulerStrategyBase(Func<Task> RunHandler)
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

        public Func<Task> RunHandler { get; }

        public Timer Timer { get; set; }
    }
}
