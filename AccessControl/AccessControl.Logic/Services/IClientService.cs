using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.AccessControl.Logic.Services
{
	public interface IClientService : IService
	{
		Task<string> CreateAsync(long companyId, string userCode, string token);
	}
}
