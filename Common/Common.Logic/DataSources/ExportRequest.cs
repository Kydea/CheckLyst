using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.DataSources
{
    public class ExportRequest : Request
    {
        public string Code { get; set; }
        public List<string> Names { get; set; }
        public List<List<string>> Records { get; set; }
        public string DecimalSeparator { get; set; }
        public string TimeZone { get; set; }
    }
}