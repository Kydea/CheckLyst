using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Instances.Terminal
{
    public interface IFTPService : IService
	{
		Task ProcessFTPMessagesAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
}
