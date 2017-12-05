using System;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Lock
	{
		public Lock(string user)
		{
			User = user;
			Date = DateTimeOffset.UtcNow;
		}

		public string User { get; }
		public DateTimeOffset Date { get; }
	}
}
