using System;
using System.Linq;

namespace CheckListManager.Common.Logic.Terminal.Scheduler
{
	public class ComputedSchedulerStrategy : SchedulerStrategyBase
	{
		public ComputedSchedulerStrategy(int[] minutes = null, int[] hours = null, int[] days = null, int[] months = null, DayOfWeek[] daysOfWeek = null)
		{
			Minutes = minutes;
			Hours = hours;
			Days = days;
			Months = months;
			DaysOfWeek = daysOfWeek;
		}

		public int[] Minutes { get; }
		public int[] Hours { get; }
		public int[] Days { get; }
		public int[] Months { get; }
		public DayOfWeek[] DaysOfWeek { get; }

		public DateTime NextExecutionTime { get; private set; }

		public override TimeSpan Interval
			=> NextExecutionTime - DateTime.UtcNow;

		public override bool ComputeNextExecutionTime(DateTime? start = null)
		{
			start = start ?? NextExecutionTime;

			DateTime
				index = start.Value > DateTime.UtcNow ? start.Value : DateTime.UtcNow,
				nextYear = DateTime.UtcNow.AddYears(1);

			index = index.AddMinutes(1);
			index = index.AddSeconds(-index.Second);
			index = index.AddMilliseconds(-index.Millisecond);

			while (index.CompareTo(nextYear) < 0)
			{
				var month = index.Month;
				if (Months == null || Months.Contains(month))
					while (index.Month == month)
					{
						var dayOfMonth = index.Day;
						if ((Days == null || Days.Contains(dayOfMonth)) && (DaysOfWeek == null || DaysOfWeek.Contains(index.DayOfWeek)))
							while (index.Day == dayOfMonth)
							{
								var hour = index.Hour;
								if (Hours == null || Hours.Contains(hour))
									while (index.Hour == hour)
									{
										var minute = index.Minute;
										if (Minutes == null || Minutes.Contains(minute))
										{
											NextExecutionTime = index;
											return true;
										}
										index = index.AddMinutes(1);
									}
								else
									index = index.AddHours(1).AddMinutes(-index.Minute);
							}
						else
							index = index.AddDays(1).AddHours(-index.Hour).AddMinutes(-index.Minute);
					}
				else
					index = index.AddMonths(1).AddDays(-index.Day + 1).AddHours(-index.Hour).AddMinutes(-index.Minute);
			}

			return false;
		}
	}
}
