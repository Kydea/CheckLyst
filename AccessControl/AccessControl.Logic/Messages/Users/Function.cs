using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
    public class Function
    {
        public long? department { get; set; }
        public long? prototypeType { get; set; }
        public string functionGroup { get; set; }
        public string function { get; set; }
        public string[] functions { get; set; }
        public string level { get; set; }
    }
}
