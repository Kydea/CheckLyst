using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Detail
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
