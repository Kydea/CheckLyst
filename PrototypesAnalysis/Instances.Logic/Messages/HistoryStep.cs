using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Logic.Messages
{
    public class HistoryStep
    {
        public long ID { get; set; }
        public int Step { get; set; }
        public string User { get; set; }
        public string AppointedUser { get; set; }
        public string Level { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset? Date { get; set; }
        public bool? Approved { get; set; }
    }
}
