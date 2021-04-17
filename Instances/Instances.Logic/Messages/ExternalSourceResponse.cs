using CheckListManager.Common.Logic.Models.Expressions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.Instances.Logic.Messages
{
	public class ExternalSourceResponse
    {
        public List<string> Columns { get; set; }
		public List<Parameter> Parameters{ get; set; }
        public class Parameter
        {
            public string Name { get; set; }
            public string Label { get; set; }
            public string ParameterType { get; set; }
            public string Value { get; set; }
            public bool ReadOnly { get; set; }
            public bool Hidden { get; set; }
        }
    }
}
