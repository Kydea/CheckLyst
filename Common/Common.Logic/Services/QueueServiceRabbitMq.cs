using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Queue;
using RabbitMQ.Client;

namespace CheckListManager.Common.Logic.Services
{
    public class QueueServiceRabbitMq : IQueueService, IDisposable
    {
        private readonly IEmailServiceBase _emailService;
        private const int ConsumerLifeSpan = 5000; // 5 Secs
        private const int MessageTtl = 60 * 60 * 1000; // 1H
        private const string DefaultExchange = "";
        private const string DeadRoutingSuffix = "-dead";
        
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public QueueServiceRabbitMq(IConnectionFactory connectionFactory, IEmailServiceBase emailService)
        {
            _emailService = emailService;
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _channel.Dispose();
                _connection.Dispose();
            }
        }
        
        public Task SendMessageAsync(string queueName, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(DefaultExchange, queueName, null, body);
            return Task.CompletedTask;
        }

        public Task SendObjectAsync<T>(string queueName, T data, CancellationToken cancellationToken = default(CancellationToken))
            => SendMessageAsync(queueName, data.ToJson(), cancellationToken);

        private async Task<T[]> ReceiveAsync<T>(string queueName, Func<string, ulong, T> handler)
        {
            try
            {
                var result = _channel.BasicGet(queueName, false);
                if (result == null)
                    return new T[0];
                
                var body = result.Body;
                var message = Encoding.UTF8.GetString(body);

                var item = handler(message, result.DeliveryTag);
                return new[] { item };
            }
            catch (Exception e)
            {
                await _emailService.LogErrorAsync(e, "Failed to receive messages");
                throw;
            }
        }

        public Task<QueueMessage[]> ReceiveMessagesAsync(string queueName, int maxNumberOfMessages = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReceiveAsync(queueName, (message, tag) => new QueueMessage(tag.ToString(), message));
        }

        public Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName, int maxNumberOfMessages = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReceiveAsync(queueName, (message, tag) => new QueueMessage<T>(tag.ToString(), message.ToObject<T>()));
        }

        public Task DeleteMessageAsync(string queueName, string messageHandle, CancellationToken cancellationToken = default(CancellationToken))
        {
            _channel.BasicAck(ulong.Parse(messageHandle), false);
            return Task.CompletedTask;
        }
    }
}