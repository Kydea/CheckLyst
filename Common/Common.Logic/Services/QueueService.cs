using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Queue;

namespace CheckListManager.Common.Logic.Services
{
	public class QueueService : IQueueService, IDisposable
	{
		private readonly IAmazonSQS _client;

		public QueueService(IAmazonSQS client)
		{
			_client = client;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// ReSharper disable once VirtualMemberNeverOverridden.Global
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_client?.Dispose();
			}
		}

		public async Task<SendMessageResponse> SendMessageAsync(string queueName, string message)
		{
			//TODO use cache
			var queue = await _client.GetQueueUrlAsync(queueName);

			return await _client.SendMessageAsync(new SendMessageRequest
			{
				QueueUrl = queue.QueueUrl,
				MessageGroupId = Guid.NewGuid().ToString(), //TODO only for fifo
				MessageDeduplicationId = Guid.NewGuid().ToString(), //TODO only for fifo
				MessageBody = message
			});
		}

		public Task<SendMessageResponse> SendObjectAsync<T>(string queueName, T data)
		{
			var message = data.ToJson();
			return SendMessageAsync(queueName, message);
		}

		public async Task<QueueMessage[]> ReceiveMessagesAsync(string queueName)
		{
			//TODO use cache
			var queue = await _client.GetQueueUrlAsync(queueName);

			var read = await _client.ReceiveMessageAsync(queue.QueueUrl);

			return read.Messages.Select(m => new QueueMessage(m.ReceiptHandle, m.Body)).ToArray();
		}

		public Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName)
		{
			return ReceiveMessagesAsync(queueName)
				.ContinueWith(
					task => task.Result.Select(m => new QueueMessage<T>(m.MessageHandle, m.Message.ToObject<T>())).ToArray()
				);
		}

		public Task DeleteMessageAsync(string queueName, string messageHandle)
		{
			//TODO use cache
			return _client.GetQueueUrlAsync(queueName)
				.ContinueWith(
					task => _client.DeleteMessageAsync(task.Result.QueueUrl, messageHandle)
				);			
		}
	}
}