using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class ConfigurationError
    {
        public ConfigurationError(string Property, string Message)
        {
            this.Property = Property;
            this.Message = Message;
        }

        public string Property { get; }
        public string Message{ get; }
    }
}
