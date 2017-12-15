using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Prototypes.Logic.Services
{
	public interface IPrototypeService : IService
	{
		Task<PrototypeResponse> OpenAsync(DefaultIdentity defaultIdentity, PrototypeRequest request);
        Task<PrototypeResponse> CloneAsync(DefaultIdentity defaultIdentity, PrototypeRequest request);
        Task<PrototypeResponse> CreateAsync(DefaultIdentity defaultIdentity, PrototypeRequest request);
        Task<PrototypeResponse> VersionAsync(DefaultIdentity defaultIdentity, PrototypeRequest request);
        VerifyExpressionsResponse VerifyExpressions(PrototypeRequest request);
        Task<PrototypeResponse> SaveAsync(DefaultIdentity defaultIdentity, PrototypeRequest request);
    }
}
