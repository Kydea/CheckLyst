using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Tools
{
    public class DateTimeOffsetConversionRow
    {
        public DateTimeOffsetConversionRow(string Value, string Format, string TimeZone)
        {
            DateTimeOffset = DateTimeOffsetHelper.Parse(Value, TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, Format);
            this.Ambiguous = Ambiguous;
            this.AmbiguousDateTimeOffsets = AmbiguousDateTimeOffsets;
        }

        public DateTimeOffset? DateTimeOffset { get; set; }
        public bool Ambiguous { get; set; }
        public TimeSpan[] AmbiguousDateTimeOffsets { get; set; }
    }
}
