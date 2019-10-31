using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Tools
{
    public class DateTimeOffsetConversionRequest
    {
        public List<DateTimeOffset> DateTimeOffsets { get; set; }
        public List<string> Strings { get; set; }
        public string TimeZone { get; set; }
        public string InputFormat { get; set; }
        public string OutputFormat { get; set; }
    }
}
