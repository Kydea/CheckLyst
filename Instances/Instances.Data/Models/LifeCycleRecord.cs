using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Data.Models
{
    public class LifeCycleRecord: OverviewRecord
    {
        public int Days { get; set; }
        public int LifeCycle { get; set; }
    }
}
