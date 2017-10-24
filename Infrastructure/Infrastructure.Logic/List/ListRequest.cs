using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.List
{
    public class ListRequest : Request
    {
        public long? Department { get; set; }

        public string PrototypeType { get; set; }
    }
}
