using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public class ListRequest
    {
        public string Name { get; set; }

        public int Department { get; set; }

        public int PrototypeType { get; set; }

        public string State { get; set; }
    }
}
