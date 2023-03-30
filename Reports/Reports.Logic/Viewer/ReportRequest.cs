using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportRequest
    {
        public long ID { get; set; }
        public List<Parameter> Parameters { get; set; }
        public long? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string UserCode { get; set; }
        public string TimeZone { get; set; }

        public class Parameter
        {
            public string Code { get; set; }
            public string Value { get; set; }
        }
    }
}
