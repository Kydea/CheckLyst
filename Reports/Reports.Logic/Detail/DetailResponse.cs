using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Detail
{
    public class DetailResponse : Form
    {
        public List<string> Groups { get; set; }
        public List<ReportColumn> Columns { get; set; }
        public List<ReportClause> Clauses { get; set; }
        public Dictionary<string, ValidationError> ColumnErrors { get; set; } = new Dictionary<string, ValidationError>();
        public Dictionary<string, ValidationError> ClauseErrors { get; set; } = new Dictionary<string, ValidationError>();

        public string CreatedBy { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
