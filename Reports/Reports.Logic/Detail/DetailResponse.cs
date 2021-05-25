using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Detail
{
    public class DetailResponse : Form
    {
        public List<ReportColumn> Columns { get; set; }
        public List<ReportClause> Clauses { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
