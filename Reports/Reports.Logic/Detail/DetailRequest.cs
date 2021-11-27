using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Detail
{
    public class DetailRequest : Request
    {
        public long? ID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string ReportSource { get; set; }
        public string Query { get; set; }
        public bool InstancesRangeRestricted { get; set; }
        public string InstancesIDAlias { get; set; }
        public bool PrototypesRangeRestricted { get; set; }
        public string PrototypesIDAlias { get; set; }
        public bool Online { get; set; }
        public bool Restricted { get; set; }

        public List<string> Groups { get; set; }
        public List<ReportColumn> Columns { get; set; }
        public List<ReportClause> Clauses { get; set; }

        public string Reason { get; set; }
    }
}
