using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Logic.List
{
    public class ListRequest : Request
    {
        public long? Department { get; set; }

        public long? PrototypeType { get; set; }

        public long? Prototype { get; set; }

        public int? Version { get; set; }

        public string Description { get; set; }

        public int? Year { get; set; }

        public int? Number { get; set; }

        public string TagCode { get; set; }

        public string TagValue { get; set; }

        public string State { get; set; }


    }
}
