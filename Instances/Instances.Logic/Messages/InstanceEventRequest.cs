using System.ComponentModel.DataAnnotations;

namespace CheckListManager.Instances.Logic.Messages
{
	public class InstanceEventRequest
	{
		[Required]
		public string Code { get; set; }
		public string Type { get; set; }

		public int? Item { get; set; }

		public string Data { get; set; }

		public bool? Simple { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Pin { get; set; }		
	}
}
