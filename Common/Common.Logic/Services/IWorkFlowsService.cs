using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using System;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Common.Logic.Services
{
	public interface IWorkFlowsService : IService
	{
		Task<long> ApplyWorkFlowAsync(DefaultIdentity defaultIdentity, long workFlow, DateTimeOffset? date, long? prototype, int? version, long? instance, string action);
	}
}
