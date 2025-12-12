using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Reports.Data.Models;
using System;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportResponse
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool Online { get; set; }
        public List<string> Columns { get; set; }
        public IEnumerable<ReportRow> Rows { get; set; }
        public bool Enqueued { get; set; }
    }
}
