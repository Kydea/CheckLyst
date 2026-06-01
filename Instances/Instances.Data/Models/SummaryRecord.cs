using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Data.Models
{
    public class SummaryRecord : OverviewRecord
    {
        public long CompanyID { get; set; }
        public string CompanyDescription { get; set; }
        public long TotalInstances { get; set; }
        public long InProgressInstances { get; set; }
        public long CompletedInstances { get; set; }
        public long ConfirmedInstances { get; set; }
        public long DeletedInstances { get; set; }
    }
}
