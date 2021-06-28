using CheckListManager.Common.Data;
using CheckListManager.Instances.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Logic.Messages
{
    public class HistoryRow
    {
        public long ID { get; set; }
        public DateTimeOffset Date { get; set; }
        public string User { get; set; }
        public List<HistoryStep> Steps { get; set; }
        public bool? Approved { get; set; }
        public string ProposedState { get; set; }
        public string Note { get; set; }
        public string State { get; set; }
    }
}
