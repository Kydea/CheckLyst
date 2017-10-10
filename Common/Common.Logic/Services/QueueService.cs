using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Queue;

namespace CheckListManager.Common.Logic.Services
{
	public class QueueService : IQueueService, IDisposable
	{
		private const string QueueRegionName = "QueueService";

		private readonly IAmazonSQS _client;
		private readonly IMemoryCache _memoryCache;

		public QueueService(IAmazonSQS client, IMemoryCache memoryCache)
		{
			_client = client;
			_memoryCache = memoryCache;
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

		public Task SendMessageAsync(string queueName, string message, CancellationToken cancellationToken = default(CancellationToken))
		{
			return
				GetQueueUrlAsync(queueName, cancellationToken)
					.ContinueWith(task =>
						_client.SendMessageAsync(new SendMessageRequest
						{
							QueueUrl = task.Result,
							MessageGroupId = Guid.NewGuid().ToString(),
							MessageDeduplicationId = Guid.NewGuid().ToString(),
							MessageBody = message
						}, cancellationToken), cancellationToken
				);
		}

		public Task SendObjectAsync<T>(string queueName, T data, CancellationToken cancellationToken = default(CancellationToken))
			=> SendMessageAsync(queueName, data.ToJson(), cancellationToken);

		public async Task<QueueMessage[]> ReceiveMessagesAsync(string queueName, int maxNumberOfMessages = 10, CancellationToken cancellationToken = default(CancellationToken))
		{
			maxNumberOfMessages = Math.Min(Math.Max(maxNumberOfMessages, 1), 10);

			var queueUrl = await GetQueueUrlAsync(queueName, cancellationToken);
			var request = new ReceiveMessageRequest { QueueUrl = queueUrl, MaxNumberOfMessages = maxNumberOfMessages };

			return await _client.ReceiveMessageAsync(request, cancellationToken)
				.ContinueWith(
					task => task.Result.Messages.Select(m => new QueueMessage(m.ReceiptHandle, m.Body)).ToArray(), cancellationToken
				);
		}

		public Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName, int maxNumberOfMessages = 10, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ReceiveMessagesAsync(queueName, maxNumberOfMessages, cancellationToken)
				.ContinueWith(
					task => task.Result.Select(m => new QueueMessage<T>(m.MessageHandle, m.Message.ToObject<T>())).ToArray(), cancellationToken
				);
		}

		public Task DeleteMessageAsync(string queueName, string messageHandle, CancellationToken cancellationToken = default(CancellationToken))
		{
			return GetQueueUrlAsync(queueName, cancellationToken)
				.ContinueWith(
					task => _client.DeleteMessageAsync(task.Result, messageHandle, cancellationToken), cancellationToken
				);
		}

		private async Task<string> GetQueueUrlAsync(string queueName, CancellationToken cancellationToken)
		{
			return await _memoryCache.GetOrAddAsync(queueName, k =>
			{
				return _client.GetQueueUrlAsync(queueName, cancellationToken).ContinueWith(task => task.Result.QueueUrl, cancellationToken);
			}, QueueRegionName);
		}
	}
}