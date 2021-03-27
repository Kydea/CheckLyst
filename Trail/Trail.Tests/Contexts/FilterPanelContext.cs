using CheckListManager.Common.Logic.Trail;
using CheckListManager.Trail.Logic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Trail.Tests.Contexts
{
    public class FilterPanelContext
    {
        public string User { get; set; }

        public string CheckListCode { get; set; }

        public long? CheckListID { get; set; }

        public List<TrailMessage> Response { get; set; }
    }
}
