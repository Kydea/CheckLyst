using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.Reports.Logic.List
{
    public class ListRequest : Request
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }
}
