using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Detail
{
    public class ReportColumn
    {
        public string Label { get; set; }
        public string Column { get; set; }
        public bool Visible { get; set; }
        public string Format { get; set; }
        public bool GroupColumn { get; set; }
        public int OrderColumn { get; set; }
        public string ValueAggregation { get; set; }
        public string GroupAggregation { get; set; }
    }
}
