using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public interface ISettingService : IService
	{
		Task<string> SmtpHostAsync(long companyId);
		Task<int> SmtpPortAsync(long companyId);
		Task<string> SmtpUsernameAsync(long companyId);
		Task<string> SmtpPasswordAsync(long companyId);
		Task<string> EmailSenderAsync(long companyId);
		Task<string> UserBlockMessageAsync(long companyId);
		Task<string> UserBlockWarningMessageAsync(long companyId);
		Task<string> SignatureMessageAsync(long companyId);
	}
}
