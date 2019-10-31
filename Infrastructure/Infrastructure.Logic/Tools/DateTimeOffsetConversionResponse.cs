using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Tools
{
    public class DateTimeOffsetConversionResponse
    {
        public Dictionary<DateTimeOffset, string> DateTimeOffsets { get; set; }
        public Dictionary<string, DateTimeOffsetConversionRow> Strings { get; set; }
    }
}
