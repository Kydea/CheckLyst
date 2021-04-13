using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MultipartDataMediaFormatter.Infrastructure;

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
		public bool Simple { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Pin { get; set; }		
		public string Note { get; set; }
        public Dictionary<string, string> Row { get; set; }
        public List<string> Columns { get; set; }
        public List<List<string>> Rows { get; set; }
        public HttpFile Attachment { get; set; }
        public List<Email> Emails { get; set; }

        public struct Email
        {
            public string Code { get; set; }
            public string Description { get; set; }
            public string Address { get; set; }
        }
	}
}
