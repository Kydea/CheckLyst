namespace CheckListManager.Prototypes.Logic.Messages
{
    public class ProcessAttachmentRequest : AttachmentRequestBase
    {
        public bool FormatAsBase64 { get; set; }
        public string Key { get; set; }
    }
}