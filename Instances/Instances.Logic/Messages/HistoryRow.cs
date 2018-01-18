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

        private string _State;
        public string State
        {
            set
            {
                _State = value;
            }
            get
            {
                return
                    _State ??
                    (
                        Approved == false ? InstanceState.Deleted :
                        Approved == true ? InstanceState.Confirmed :
                        InstanceState.Completed
                    );
            }
        }
    }
}
