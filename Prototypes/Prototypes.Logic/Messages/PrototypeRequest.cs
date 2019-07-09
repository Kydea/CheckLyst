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
        public string State { get; set; }
        public bool StateProposed { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
        public long? InstanceConfirmationWorkFlow { get; set; }

        public long? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string UserCode { get; set; }
        public string TimeZone { get; set; }

        public Guid? AttachmentTemporaryKey { get; set; }
    }
}
