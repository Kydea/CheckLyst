using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Detail
{
    public class ReportClause
    {
        public string Label { get; set; }
        public string Clause { get; set; }
        public bool Visible { get; set; }
        public bool Editable { get; set; }
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }
        public string ClauseType { get; set; }

    }
}
