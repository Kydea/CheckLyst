using System.Threading.Tasks;
using Amazon.SQS.Model;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Queue;

namespace CheckListManager.Common.Logic.Services
{
	public interface IQueueService : IService
	{
		Task<SendMessageResponse> SendMessageAsync(string queueName, string message);
		Task<SendMessageResponse> SendObjectAsync<T>(string queueName, T data);

		Task<QueueMessage[]> ReceiveMessagesAsync(string queueName);
		Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName);

		Task DeleteMessageAsync(string queueName, string messageHandle);
	}
}
