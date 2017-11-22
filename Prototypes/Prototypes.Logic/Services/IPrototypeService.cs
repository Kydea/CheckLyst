using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Prototypes.Logic.Messages;

namespace CheckListManager.Prototypes.Logic.Services
{
	public interface IPrototypeService : IService
	{
		Task<GetPrototypeResponse> GetAsync(long companyId, GetPrototypeRequest request);
	}
}
