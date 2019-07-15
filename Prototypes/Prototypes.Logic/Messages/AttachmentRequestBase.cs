using System;

namespace CheckListManager.Prototypes.Logic.Messages
{
    public abstract class AttachmentRequestBase
    {
        public long Id { get; set; }
        public int Version { get; set; }

        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
		
        public Guid? AttachmentTemporaryKey { get; set; }
        
    }
}