using System.Collections.Generic;

namespace CheckListManager.Instances.Logic.Messages
{
	public class CreateInstanceRequest
	{
		public long Prototype { get; set; }

        public int? Version { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
	}
}
