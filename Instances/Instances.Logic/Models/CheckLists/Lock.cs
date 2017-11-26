using System;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Lock
	{
		public Lock(string user, DateTimeOffset date)
		{
			User = user;
			Date = date;
		}

		public string User { get; }
		public DateTimeOffset Date { get; }
	}
}
