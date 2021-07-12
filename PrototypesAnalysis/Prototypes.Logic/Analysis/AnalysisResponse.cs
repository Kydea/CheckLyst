using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;

namespace CheckListManager.Prototypes.Logic.Analysis
{
    public class AnalysisResponse
    {
        public DateTimeOffset Start { get; } = DateTimeOffset.Now;
        public DateTimeOffset End { get; set; }
        public TimeSpan ElapsedTime => End - Start;
        public Dictionary<string, CountRecord> Counts { get; } = new Dictionary<string, CountRecord>();
        public Dictionary<string, List<string>> Errors { get; } = new Dictionary<string, List<string>>();

        public class CountRecord {
            public int KO { get; set; }
            public int OK { get; set; }
        }
    }
}
