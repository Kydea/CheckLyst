using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportRequest
    {
        public string Code { get; set; }
        public List<Parameter> Parameters { get; set; }

        public class Parameter
        {
            public string Code { get; set; }
            public string Value { get; set; }
        }
    }
}
