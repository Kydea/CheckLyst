using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Web.ClassExtensions
{
    public static class DateTimeOffsetExtension
    {
        public static DateTimeOffset? ToTimeZone(this DateTimeOffset? DateTimeOffset, string TimeZone)
        {
            if (TimeZone == null) throw new ArgumentNullException();
            return DateTimeOffset == null ? (DateTimeOffset?)null : TimeZoneInfo.ConvertTime(DateTimeOffset.Value, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
        }

        public static DateTimeOffset ToTimeZone(this DateTimeOffset DateTimeOffset, string TimeZone)
        {
            if (TimeZone == null) throw new ArgumentNullException();
            return TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
        }
    }
}
