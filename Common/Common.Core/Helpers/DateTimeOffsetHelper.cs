using System;
using System.Globalization;

namespace CheckListManager.Common.Core.Helpers
{
    public static class DateTimeOffsetHelper
    {
        public static readonly string DateFormat = "yyyy-MM-dd";
        public static readonly string TimeFormat = "HH:mm";

        public static DateTimeOffset ToLocalTime(this DateTimeOffset DateTimeOffset, string TimeZone)
        {
            if (TimeZone == null) throw new ArgumentNullException();
            return TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
        }

        public static string Format(this DateTimeOffset DateTimeOffset, string TimeZone, string Format = null) =>
            DateTimeOffset.ToLocalTime(TimeZone).ToString(Format ?? $"{DateFormat} {TimeFormat} UTC zzz");

        public static DateTimeOffset? Parse(string Value, string TimeZone, out bool Ambiguous, out TimeSpan[] AmbiguousDateTimeOffsets, params string[] Format)
        {
            if (string.IsNullOrEmpty(Value))
            {
                Ambiguous = false;
                AmbiguousDateTimeOffsets = null;
                return null;
            }

            if (Format == null || Format.Length == 0) Format = new string[] { DateFormat, $"{DateFormat} {TimeFormat}" };

            var DateTime = System.DateTime.ParseExact(Value, Format, null, DateTimeStyles.None);
            var TimeZoneInfo = System.TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
            var UTCDateTimeOffset = System.TimeZoneInfo.ConvertTime(DateTime, TimeZoneInfo.Utc);
            var DateTimeOffset = new DateTimeOffset(UTCDateTimeOffset, TimeSpan.Zero);
            Ambiguous = TimeZoneInfo.IsAmbiguousTime(DateTime);
            AmbiguousDateTimeOffsets = Ambiguous ? TimeZoneInfo.GetAmbiguousTimeOffsets(DateTime) : null;

            return DateTimeOffset;  
        }
    }
}
