using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Prototypes.Logic.Messages;

namespace CheckListManager.Prototypes.Logic.Services
{
	public interface IPrototypeService : IService
	{
		Task<GetPrototypeResponse> OpenAsync(long companyId, GetPrototypeRequest request);
        Task<GetPrototypeResponse> CloneAsync(long companyId, GetPrototypeRequest request);
        Task<GetPrototypeResponse> CreateAsync(long companyId, GetPrototypeRequest request);
        Task<GetPrototypeResponse> VersionAsync(long companyId, GetPrototypeRequest request);
    }
}
