using CheckListManager.Common.Logic.DataEngine;
using System;

namespace CheckListManager.Instances.Logic.List
{
    public class ListRequest : Request
    {
        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
        public long? Prototype { get; set; }
        public int? Version { get; set; }
        public string Description { get; set; }
        public int? Year { get; set; }
        public int? Number { get; set; }
        public string TagCode { get; set; }
        public string TagValue { get; set; }
        public string State { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public DateTimeOffset? WholeTo { get { return To?.AddDays(1); } }
        public string ExceptionState { get; set; }
        public string ExceptionDescription { get; set; }
        public bool ShowException { get; set; }

    }
}
