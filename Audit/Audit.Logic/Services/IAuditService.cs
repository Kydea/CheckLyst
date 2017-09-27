using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Audit.Logic.Services
{
	public interface IAuditService : IService
	{
		Task ProcessMessagesAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
}
