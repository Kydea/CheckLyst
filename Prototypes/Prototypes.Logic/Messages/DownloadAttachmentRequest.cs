using System;

namespace CheckListManager.Prototypes.Logic.Messages
{
    public class DownloadAttachmentRequest
    {
        public bool FormatAsBase64 { get; set; }
        
        public long Id { get; set; }
        public int Version { get; set; }

        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
		
        public Guid? AttachmentTemporaryKey { get; set; }
        
        public string Key { get; set; }
    }
}