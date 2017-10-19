using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Trail.Logic.List
{
    public class ListRequest : Request
    {
        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }

        public string User { get; set; }

        public string CheckList { get; set; }

        public string Action { get; set; }

        public string Information { get; set; }
    }
}
