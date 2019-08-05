using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;

namespace CheckListManager.Common.Logic.Services
{
	public interface ISettingService : IService
	{
        Task<string> LDAPHostAsync(long companyId);
        Task<string> SmtpHostAsync(long companyId);
		Task<int> SmtpPortAsync(long companyId);
		Task<string> SmtpUsernameAsync(long companyId);
		Task<string> SmtpPasswordAsync(long companyId);
		Task<string> EmailSenderAsync(long companyId);
		Task<string> UserBlockMessageAsync(long companyId);
		Task<string> UserBlockWarningMessageAsync(long companyId);
		Task<string> SignatureMessageAsync(long companyId);
		Task<string> InstanceMessageAsync(long companyId);
        Task<string> PrototypesFtpConnectionAsync(long companyId);
        Task<string> InstancesFtpConnectionAsync(long companyId);
        Task<string> DecimalSeparatorAsync(long companyId);
        Task<TimeZones> TimeZoneAsync(long companyId);
        Task<bool> RequireUserReasonAsync(long companyId);
    }
}
