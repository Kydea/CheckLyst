using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Detail
{
    public class ReportClause
    {
        public string Label { get; set; }
        public ClauseRecord Clause { get; set; }
        public bool Visible { get; set; }
        public bool Editable { get; set; }
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }
        public string ClauseType { get; set; }

        public class ClauseRecord
        {
            public string Column { get; set; }
            public string Sign { get; set; }
            public string Value { get; set; }
            public string Custom { get; set; }
        }
    }
}
