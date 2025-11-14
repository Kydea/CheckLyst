using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.License;

namespace CheckListManager.Common.Logic.Services
{
	public interface ISettingService : IService
	{
        void Clear(long companyId);
        Task<string> LDAPHostAsync(long companyId);
        Task<int> LDAPPortAsync(long companyId);
        Task<string> SmtpHostAsync(long companyId);
		Task<int> SmtpPortAsync(long companyId);
		Task<string> SmtpUsernameAsync(long companyId);
		Task<string> SmtpPasswordAsync(long companyId);
		Task<string> EmailSenderAsync(long companyId);
		Task<string> UserBlockMessageAsync(long companyId);
		Task<string> UserBlockWarningMessageAsync(long companyId);
		Task<string> SignatureMessageAsync(long companyId);
		Task<string> InstanceMessageAsync(long companyId);
        Task<string> NotificationMessageAsync(long companyId);
        Task<string> PrototypesFtpConnectionAsync(long companyId);
        Task<string> InstancesFtpConnectionAsync(long companyId);
        Task<string> DecimalSeparatorAsync(long companyId);
        Task<string> TimeZoneAsync(long companyId);
        Task<bool> RequireUserReasonAsync(long companyId);
        Task<int> InactivityTimeoutSecondsAsync(long companyId);
        Task<int> MaximumNumberOfPasswordFailureAsync(long companyId);
        Task<int> MaximumNumberOfPINFailureAsync(long companyId);
        Task<int> PINDaysLifeAsync(long companyId);
        Task<int> WarningUserInactivityDaysAsync(long companyId);
        Task<int> MaximumUserInactivityDaysAsync(long companyId);
        Task<int> PrototypesAttachmentMaxSizeBytesAsync(long companyId);
        Task<int> InstancesAttachmentMaxSizeBytesAsync(long companyId);
        Task<LicenseModel> LicenseAsync(long companyId = 1);
    }
}
