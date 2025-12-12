using CheckListManager.Common.Logic.DataEngine;
using System;

namespace CheckListManager.Reports.Logic.Run
{
    public class ListRequest : Request
    {
        public int? Report { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
