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
            var DateTime = System.DateTime.ParseExact(Value, Format ?? "yyyy-MM-dd HH:mm", null);
            var TimeZoneInfo = System.TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
            var UTCDateTimeOffset = System.TimeZoneInfo.ConvertTime(DateTime, TimeZoneInfo.Utc);
            DateTimeOffset = new DateTimeOffset(UTCDateTimeOffset, TimeSpan.Zero);
            Ambiguous = TimeZoneInfo.IsAmbiguousTime(DateTime);
            if (Ambiguous) AmbiguousDateTimeOffsets = TimeZoneInfo.GetAmbiguousTimeOffsets(DateTime);
        }

        public DateTimeOffset DateTimeOffset { get; set; }
        public bool Ambiguous { get; set; }
        public TimeSpan[] AmbiguousDateTimeOffsets { get; set; }
    }
}
