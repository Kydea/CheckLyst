using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportResponse
    {
        public string Description { get; set; }
        public List<string> Columns { get; set; }
        public IEnumerable<List<string>> Rows { get; set; }
    }
}
