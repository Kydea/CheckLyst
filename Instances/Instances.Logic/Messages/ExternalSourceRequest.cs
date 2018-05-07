using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.Instances.Logic.Messages
{
	public class ExternalSourceRequest
    {
		public string InstanceCode { get; set; }
		public string ExternalSourceCode { get; set; }
		public Dictionary<string, string> Parameters{ get; set; }
	}
}
