using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Trail.Logic.Services
{
	public interface ITrailService : IService
	{
		Task ProcessMessagesAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
}
