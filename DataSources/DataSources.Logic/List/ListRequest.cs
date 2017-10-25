using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.List
{
    public class ListRequest : Request
    {
        public string Name { get; set; }

        public long? Department { get; set; }

        public long? PrototypeType { get; set; }

        public string State { get; set; }
    }
}
