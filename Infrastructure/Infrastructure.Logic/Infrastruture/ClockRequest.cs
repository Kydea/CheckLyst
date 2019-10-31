using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Information
{
    public class ClockRequest
    {
        public DateTimeOffset LocalTime { get; set; }
        public string TimeZone { get; set; }
    }
}
