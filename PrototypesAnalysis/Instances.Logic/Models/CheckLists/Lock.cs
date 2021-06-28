    using System;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Lock
	{
		public Lock(string user)
		{
			User = user;
			LastUpdate = Date = DateTimeOffset.UtcNow;
		}

        public Lock()
        {

        }

		public string User { get; set; }
		public DateTimeOffset Date { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
    }
}
