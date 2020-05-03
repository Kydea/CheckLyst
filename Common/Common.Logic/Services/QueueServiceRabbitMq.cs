using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Queue;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

        private static string DeadLetterQueue(string queueName) => $"{queueName}{DeadRoutingSuffix}";

        private static void QueueDeclare(IModel channel, string queueName)
        {
            var arguments = new Dictionary<string, object>
            {
                { "x-message-ttl", MessageTtl },
                { "x-dead-letter-exchange", DefaultExchange },
                { "x-dead-letter-routing-key", DeadLetterQueue(queueName) }
            };

            channel.QueueDeclare(queueName,
                true,
                false,
                false,
                arguments);

            channel.QueueDeclare(DeadLetterQueue(queueName),
                true,
                false,
                false,
                null);
        }
        
        public Task SendMessageAsync(string queueName, string message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            QueueDeclare(_channel, queueName);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(DefaultExchange,
                queueName,
                null,
                body);

            return Task.CompletedTask;
        }

        public Task SendObjectAsync<T>(string queueName, T data, CancellationToken cancellationToken = default(CancellationToken))
            => SendMessageAsync(queueName, data.ToJson(), cancellationToken);

        private async Task<T[]> ReceiveAsync<T>(string queueName, Func<string, BasicDeliverEventArgs, T> handler, CancellationToken cancellationToken)
        {
            try
            {
                var list = new List<T>();
            
                QueueDeclare(_channel, queueName);
                
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    list.Add(handler(message, ea));
                };
                var tag = _channel.BasicConsume(queueName,
                    false,
                    consumer);
            
                await Task.Delay(ConsumerLifeSpan, cancellationToken);
                _channel.BasicCancel(tag);
            
                return list.ToArray();
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
            return ReceiveAsync(queueName,
                (message, ea) => new QueueMessage(ea.DeliveryTag.ToString(), message),
                cancellationToken);
        }

        public Task<QueueMessage<T>[]> ReceiveObjectsAsync<T>(string queueName, int maxNumberOfMessages = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReceiveAsync(queueName,
                (message, ea) => new QueueMessage<T>(ea.DeliveryTag.ToString(), message.ToObject<T>()),
                cancellationToken);
        }

        public Task DeleteMessageAsync(string queueName, string messageHandle,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _channel.BasicAck(ulong.Parse(messageHandle), false);
            return Task.CompletedTask;
        }
    }
}