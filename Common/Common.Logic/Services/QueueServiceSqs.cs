using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Queue;

namespace CheckListManager.Common.Logic.Services
{
	public class QueueServiceSqs : IQueueService, IDisposable
	{
		private const string QueueRegionName = "QueueService";
		private const int DefaultTimeout = 100; // Seconds

		private readonly IAmazonSQS _client;
		private readonly IMemoryCache _memoryCache;
        private readonly IConfigurationService _configurationService;
        private readonly IEmailServiceBase _emailService;

        public QueueServiceSqs(
	        IAmazonSQS client,
	        IMemoryCache memoryCache,
	        IConfigurationService configurationService,
	        IEmailServiceBase emailService)
		{
			_client = client;
			_memoryCache = memoryCache;
            _configurationService = configurationService;
            _emailService = emailService;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) _client?.Dispose();
		}

		public async Task SendMessageAsync(string queueName, string message, CancellationToken cancellationToken = default(CancellationToken))
		{
			var url = await GetQueueUrlAsync(queueName);
			await WithTimeout(async token =>
				await _client.SendMessageAsync(new SendMessageRequest
				{
					QueueUrl = url,
					MessageGroupId = Guid.NewGuid().ToString(),
					MessageDeduplicationId = Guid.NewGuid().ToString(),
					MessageBody = message
				}, token));
		}

		public Task SendObjectAsync<T>(string queueName, T data, CancellationToken cancellationToken = default(CancellationToken))
			=> SendMessageAsync(queueName, data.ToJson(), cancellationToken);

		public async Task<QueueMessage[]> ReceiveMessagesAsync(string queueName, int maxNumberOfMessages = 10, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				maxNumberOfMessages = Math.Min(Math.Max(maxNumberOfMessages, 1), 10);

				var queueUrl = await GetQueueUrlAsync(queueName);
				var request = new ReceiveMessageRequest { QueueUrl = queueUrl, MaxNumberOfMessages = maxNumberOfMessages };

				return await WithTimeout(async token =>
				{
					var response = await _client.ReceiveMessageAsync(request, token);
					return response.Messages.Select(m => new QueueMessage(m.ReceiptHandle, m.Body)).ToArray();
				});
			}
			catch (Exception e)
			{
				await _emailService.LogErrorAsync(e, "Failed to receive messages");
				throw;
			}
		}

		public async Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName, int maxNumberOfMessages = 10, CancellationToken cancellationToken = default(CancellationToken))
		{
			var messages = await ReceiveMessagesAsync(queueName, maxNumberOfMessages, cancellationToken);
			return messages.Select(m => new QueueMessage<T>(m.MessageHandle, m.Message.ToObject<T>())).ToArray();
		}

		public async Task DeleteMessageAsync(string queueName, string messageHandle, CancellationToken cancellationToken = default(CancellationToken))
		{
			var url = await GetQueueUrlAsync(queueName);
			await WithTimeout(async token => await _client.DeleteMessageAsync(url, messageHandle, token));
		}

		private async Task<string> GetQueueUrlAsync(string queueName)
		{
			return await _memoryCache.GetOrAddAsync(queueName, async k =>
			{
				return await WithTimeout(async token =>
				{
					var response = await _client.GetQueueUrlAsync(_configurationService.SqsPrefix + queueName,
						token);
					return response.QueueUrl;
				});
			}, QueueRegionName);
		}

		private static async Task<T> WithTimeout<T>(Func<CancellationToken, Task<T>> fn)
		{
			var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeout));

			try
			{
				return await fn(tokenSource.Token);
			}
			catch (OperationCanceledException exception) when (exception.CancellationToken == tokenSource.Token)
			{
				throw new TimeoutException("Timeout connecting to SQS", exception);
			}
		}
	}
}