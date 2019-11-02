using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Information
{
    public class ClockResponse
    {
        public DateTimeOffset ServerTime { get; set; }
        public DateTimeOffset LocalTime { get; set; }
    }
}
