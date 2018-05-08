using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.Instances.Logic.Messages
{
	public class InstanceEventRequest
	{
		[Required]
		public string Code { get; set; }
		[Required]
		public string Type { get; set; }

		public int? ItemId { get; set; }

        public string Value { get; set; }

		public bool? Simple { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Pin { get; set; }		
		public string Note { get; set; }

        public int? RowIndex { get; set; }
        public Dictionary<string, string> Row { get; set; }
	}
}
