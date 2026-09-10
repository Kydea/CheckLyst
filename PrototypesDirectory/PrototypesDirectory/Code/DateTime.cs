using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Matematici
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset ToLocalTime(this DateTimeOffset DateTime, string TimeZone)
        {
            if (TimeZone == null) throw new ArgumentNullException();
            return TimeZoneInfo.ConvertTime(DateTime, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
        }
    }
}
