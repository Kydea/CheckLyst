using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Prototypes.Logic.Services
{
	public interface IPrototypeService : IService
	{
		Task<PrototypeResponse> OpenAsync(long companyId, PrototypeRequest request);
        Task<PrototypeResponse> CloneAsync(long companyId, PrototypeRequest request);
        Task<PrototypeResponse> CreateAsync(long companyId, PrototypeRequest request);
        Task<PrototypeResponse> VersionAsync(long companyId, PrototypeRequest request);
        VerifyExpressionsResponse VerifyExpressions(PrototypeRequest request);
        Task<PrototypeResponse> SaveAsync(DefaultIdentity defaultIdentity, PrototypeRequest request);
    }
}
