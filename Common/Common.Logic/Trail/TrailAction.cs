namespace CheckListManager.Common.Logic.Trail
{
	public static class TrailAction
	{
        public const string Login = "Login";
        public const string LoginFailed = "Login failed";
        public const string Logout = "Logout";

        public const string UserCreated = "User created";
        public const string UserEdited = "User edited";
        public const string UserDeleted = "User deleted";

        public const string PrototypeCreated = "Prototype created";
        public const string PrototypeCtateChanged = "Prototype state changed";
        public const string PrototypeStateAutoChanged = "Prototype state auto-changed";
        public const string PrototypeStateChangedProposed = "Prototype state changed proposed";
        public const string PrototypeSignatureIssued = "Prototype signature issued";

        public const string EmailFailed = "Email failed";
    }
}
