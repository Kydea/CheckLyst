namespace CheckListManager.Common.Logic.Trail
{
	public static class TrailAction
	{
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string LoginFailed = "Login failed";

        public const string UserCreated = "User created";
        public const string UserEdited = "User edited";
        public const string UserDeleted = "User deleted";

        public const string EmailFailed = "Email failed";

        public const string PrototypeCreated = "Prototype created";
        public const string PrototypeDeleted = "Prototype deleted";
        public const string PrototypeStateChanged = "Prototype state changed";
        public const string PrototypeStateAutoChanged = "Prototype state auto-changed";
        public const string PrototypeStateChangeProposed = "Prototype state change proposed";
        public const string PrototypeSignatureIssued = "Prototype signature issued";

        public const string InstanceCreated = "Instance created";
        public const string InstanceOpened = "Instance opened";
        public const string InstanceQuitted = "Instance quitted";
        public const string InstanceStateChanged = "Instance state changed";
        public const string InstanceSignatureIssued = "Instance signature issued";
        public const string InstanceSignatureCanceled = "Instance signature canceled";
        public const string InstanceValueChanged = "Instance value changed";
        public const string InstanceValueCopied = "Instance value copied";
        public const string InstanceIterationAdded = "Instance iteration added";
        public const string InstanceIterationDeleted = "Instance iteration deleted";
        public const string InstanceIterationMovedUp = "Instance iteration moved up";
        public const string InstanceIterationMovedDown = "Instance iteration moved down";

        public const string InstanceExternalSourceRowChanged = "Instance external source row changed";

        public const string InstanceExportCompleted = "Instance export completed";
        public const string InstanceExportFailed = "Instance export failed";
        public const string PrototypeExportCompleted = "Prototype export completed";
        public const string PrototypeExportFailed = "Prototype export failed";

        public const string JobError = "Job error";
    }
}
