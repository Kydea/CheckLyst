using CheckListManager.Reports.Logic.Detail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Tests.Contexts
{
    public class ReportsContext
    {
        public DetailRequest Request { get; set; }

        public DetailResponse Response { get; set; }
    }
}
