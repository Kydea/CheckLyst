using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.Prototypes.Data.Models
{
    public class PrototypeFilters : Request
    {
        public string Name { get; set; }
        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
        public long? ID { get; set; }
        public string State { get; set; }
        public string Pending { get; set; }
    }
}
