using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Infrastructure.Logic.Email
{
	public interface IEmailService : IService
	{
		Task ProcessEmailMessagesAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
}
