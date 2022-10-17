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
        public const string UserBanned = "User banned";
        public const string UserDisabled = "User disabled";

        public const string PINBanned = "PIN banned";
        public const string PINDisabled = "PIN disabled";

        public const string GroupCreated = "Group created";
        public const string GroupEdited = "Group edited";
        public const string GroupDeleted = "Group deleted";

        public const string RoleCreated = "Role created";
        public const string RoleEdited = "Role edited";
        public const string RoleDeleted = "Role deleted";

        public const string EmailFailed = "Email failed";
        public const string EmailSent = "Email sent";

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
        public const string InstanceSignatureFailed = "Instance signature failed";
        public const string InstanceSignatureCanceled = "Instance signature canceled";
        public const string InstanceSignatureCancelationFailed = "Instance signature cancelation failed";
        public const string InstanceValueChanged = "Instance value changed";
        public const string InstanceValueCopied = "Instance value copied";
        public const string InstanceIterationAdded = "Instance iteration added";
        public const string InstanceIterationDeleted = "Instance iteration deleted";
        public const string InstanceIterationMovedUp = "Instance iteration moved up";
        public const string InstanceIterationMovedDown = "Instance iteration moved down";
        public const string InstanceDownloaded = "Instance downloaded";

        public const string InstanceExternalSourceRowChanged = "Instance external source row changed";
        public const string InstanceExternalSourceRowImported = "Instance external source row imported";

        public const string InstanceExportCompleted = "Instance export completed";
        public const string InstanceExportFailed = "Instance export failed";
        public const string PrototypeExportCompleted = "Prototype export completed";
        public const string PrototypeExportFailed = "Prototype export failed";
        public const string PrototypeDownloaded = "Prototype downloaded";

        public const string JobError = "Job error";
        
        public const string WorkflowSignatureIssued = "Workflow signature issued";
        public const string WorkflowSignatureFailed = "Workflow signature failed";
        
        public const string WorkflowCreated = "Workflow created";
        public const string WorkflowUpdated = "Workflow updated";
        public const string WorkflowDeleted = "Workflow deleted";
        
        public const string DataSourceCreated = "DataSource created";
        public const string DataSourceUpdated = "DataSource updated";
        public const string DataSourceDeleted = "DataSource deleted";

        public const string CompanyCreated = "Company created";
        public const string CompanyUpdated = "Company updated";

        public const string DepartmentCreated = "Department created";
        public const string DepartmentUpdated = "Department updated";
        public const string DepartmentDeleted = "Department deleted";

        public const string PrototypeTypeCreated = "PrototypeType created";
        public const string PrototypeTypeUpdated = "PrototypeType updated";
        public const string PrototypeTypeDeleted = "PrototypeType deleted";

        public const string FtpServerCreated = "FTPServer created";
        public const string FtpServerUpdated = "FTPServer updated";
        public const string FtpServerDeleted = "FTPServer deleted";

        public const string ReportCreated = "Report created";
        public const string ReportUpdated = "Report updated";
        public const string ReportDeleted = "Report deleted";
    }
}
