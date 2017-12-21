using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Data
{
    public static class WorkFlowAction
    {
        public const string PrototypeToApproved = "Prototype – state to Approved/Active";
        public const string PrototypeToObsolete = "Prototype – state to Obsolete";
        public const string InstanceToConfirmed = "Instance – state to Confirmed";
    }
}
