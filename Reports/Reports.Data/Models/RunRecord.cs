using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Data.Models
{
    public class RunRecord
    {
        public long ID { get; set; }
        public long ReportID { get; set; }
        public string ReportCode { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? CompletedOn { get; set; }
        public bool Completed => CompletedOn.HasValue;
    }
}
