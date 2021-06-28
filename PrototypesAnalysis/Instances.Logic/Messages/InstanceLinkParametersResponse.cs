using CheckListManager.Common.Logic.Models.Expressions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.Instances.Logic.Messages
{
	public class InstanceLinkParametersResponse
    {
		public List<Parameter> Parameters{ get; set; }
        public class Parameter
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Error { get; set; }
        }
    }
}
