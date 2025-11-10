using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Trail
{
    public class TrailRow
    {
        public long Company { get; set; }
        public long ID { get; set; }
        public string User { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public string Action { get; set; }
        public string Information { get; set; }
        public string ShortInformation { get; set; }
        public string CheckListCode { get; set; }
        public long? CheckListID { get; set; }
    }
}