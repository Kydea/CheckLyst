using CheckListManager.Prototypes.Logic.Models.CheckLists;
using System;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class PrototypeResponse
	{
		public long ID { get; set; }
		public int Version { get; set; }
        public Prototype CheckList { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }

        public string State { get; set; }
        public string ProposedState { get; set; }
        public DateTimeOffset? ProposedDate { get; set; }
        
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? Department { get; set; }
        public string DepartmentName { get; set; }
        public long? PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public long? InstanceConfirmationWorkFlow { get; set; }

        public Guid? AttachmentTemporaryKey { get; set; }
    }
}
