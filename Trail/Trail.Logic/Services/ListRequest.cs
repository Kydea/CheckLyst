using System;
using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.Trail.Logic.Services
{
    public class ListRequest : Request
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public DateTimeOffset? WholeTo { get { return To?.AddDays(1); } }
        public string User { get; set; }
        public string CheckList { get; set; }
        public string Action { get; set; }
        public string Information { get; set; }
        public long? CompanyId { get; set; }
        public string TimeZone { get; set; }
    }
}
