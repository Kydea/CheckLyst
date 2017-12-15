using CheckListManager.Prototypes.Logic.Models.CheckLists;
using System;

namespace CheckListManager.Prototypes.Logic.Messages
{
    public class PrototypeRequest
    {
        public long ID { get; set; }
        public int Version { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Prototype CheckList { get; set; }
    }
}
