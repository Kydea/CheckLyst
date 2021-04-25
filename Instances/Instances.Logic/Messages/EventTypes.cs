namespace CheckListManager.Instances.Logic.Messages
{
    public static class EventTypes
    {
        public const string PageChange = nameof(PageChange);
        public const string ValueChange = nameof(ValueChange);
        public const string ValueCopy = nameof(ValueCopy);
        public const string Add = nameof(Add);
        public const string Remove = nameof(Remove);
        public const string Up = nameof(Up);
        public const string Down = nameof(Down);
        public const string Signature = nameof(Signature);
        public const string SignatureCheck = nameof(SignatureCheck);
        public const string SignatureCancelation = nameof(SignatureCancelation);
        public const string UnlockPage = nameof(UnlockPage);
        public const string CountDown = nameof(CountDown);
        public const string SkipCountDown = nameof(SkipCountDown);
        public const string Refresh = nameof(Refresh);
        public const string Quit = nameof(Quit);
        public const string ExternalSourceRowChange = nameof(ExternalSourceRowChange);
        public const string SendEmail = nameof(SendEmail);
        public const string UploadAttachment = nameof(UploadAttachment);
        public const string Import = nameof(Import);
        public const string AttachInstance = nameof(AttachInstance);
    }
}