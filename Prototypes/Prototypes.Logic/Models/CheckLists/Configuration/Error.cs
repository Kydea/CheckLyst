using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists.Configuration
{
    public class Error
    {
        public Error(string Property, string Message)
        {
            this.Property = Property;
            this.Message = Message;
        }

        public string Property { get; }
        public string Message{ get; }
    }
}
