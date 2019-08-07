using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.DataSources.Tests.Setup
{
    public class EmptyQueueService : IQueueService
    {
        public Task SendMessageAsync(string queueName, string message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task SendObjectAsync<T>(string queueName, T data, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task<QueueMessage[]> ReceiveMessagesAsync(string queueName, int maxNumberOfMessages = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName, int maxNumberOfMessages = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteMessageAsync(string queueName, string messageHandle,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}