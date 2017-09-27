using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Queue;

namespace CheckListManager.Common.Logic.Services
{
	public interface IQueueService : IService
	{
		Task SendMessageAsync(string queueName, string message, CancellationToken cancellationToken = default(CancellationToken));
		Task SendObjectAsync<T>(string queueName, T data, CancellationToken cancellationToken = default(CancellationToken));

		Task<QueueMessage[]> ReceiveMessagesAsync(string queueName, CancellationToken cancellationToken = default(CancellationToken));
		Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName, CancellationToken cancellationToken = default(CancellationToken));

		Task DeleteMessageAsync(string queueName, string messageHandle, CancellationToken cancellationToken = default(CancellationToken));
	}
}
