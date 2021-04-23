using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.Prototypes.Logic.List
{
    public class ListRequest : Request
    {
        public string Name { get; set; }
        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
        public long? ID { get; set; }
        public string State { get; set; }
        public string Pending { get; set; }
    }
}
