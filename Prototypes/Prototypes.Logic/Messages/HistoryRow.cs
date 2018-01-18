using CheckListManager.Common.Data;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;

namespace CheckListManager.Prototypes.Logic.Messages
{
    public class HistoryRow
    {
        public long ID { get; set; }
        public DateTimeOffset Date { get; set; }
        public string User { get; set; }
        public List<HistoryStep> Steps { get; set; }
        public bool? Approved { get; set; }
        public bool HasWorkFlow { private get; set; }
        public string ProposedState { get; set; }
        public string State { get; set; }
    }
}
