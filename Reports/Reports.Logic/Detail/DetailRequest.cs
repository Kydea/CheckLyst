using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.Reports.Logic.Detail
{
    public class DetailRequest : Request
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Class { get; set; }
        public string Password { get; set; }
        public string Parameters { get; set; }
        
        public string Reason { get; set; }
    }
}
