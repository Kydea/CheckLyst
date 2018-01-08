using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Common.Logic.WorkFlows
{
	public interface IWorkFlowsService : IService
	{
		Task<CreateResponse> CreateWorkFlowApplicationAsync(IDefaultIdentity identity, CreateRequest request);
	}
}
