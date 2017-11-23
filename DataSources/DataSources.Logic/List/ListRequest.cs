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
        public string Code { get; set; }

        public string Description { get; set; }
    }
}
