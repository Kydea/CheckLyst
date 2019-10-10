using System;

namespace CheckListManager.Common.Core.Helpers
{
    public static class DateTimeOffsetHelper
    {
        public static DateTimeOffset ToLocalTime(this DateTimeOffset DateTimeOffset, string TimeZone)
        {
            if (TimeZone == null) throw new ArgumentNullException();
            return TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
        }

        public static string Format(this DateTimeOffset DateTimeOffset, string TimeZone) =>
            DateTimeOffset.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm UTC zzz");
    }
}
