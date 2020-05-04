using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Services;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

namespace CheckListManager.Trail.QueueTests
{
    public class RabbitMqTests
    {
        private readonly IEmailServiceBase _emailService = Substitute.For<IEmailServiceBase>();
        private QueueServiceRabbitMq _queueService;

        [SetUp]
        public void Setup()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "rabbitmq.clm.internal",
                UserName = "CheckListManager",
                Password = "CheckListManager01"
            };
            _queueService = new QueueServiceRabbitMq(connectionFactory, _emailService);
        }

        [Test]
        public void CanPublishMessages()
        {
            _queueService.SendMessageAsync("tests", "Test Message").Wait();
            var messages = _queueService.ReceiveMessagesAsync("tests").Result;
            
            Assert.IsTrue(messages.Length > 0);
            Assert.AreEqual("Test Message", messages[0].Message);

            foreach (var message in messages)
            {
                _queueService.DeleteMessageAsync("tests", message.MessageHandle).Wait();
            }
        }
    }
}