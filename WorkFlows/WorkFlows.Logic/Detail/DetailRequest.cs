using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Detail
{
    public class DetailRequest
    {
        public long? ID { get; set; }
        public string Description { get; set; }
        public Step[] Steps { get; set; }
    }
}
