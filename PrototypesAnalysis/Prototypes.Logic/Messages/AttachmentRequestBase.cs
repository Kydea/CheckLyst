using System;

namespace CheckListManager.Prototypes.Logic.Messages
{
    public abstract class AttachmentRequestBase
    {
        public long Id { get; set; }
        public int Version { get; set; }
    }
}