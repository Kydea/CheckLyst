using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;
using NLog;

namespace CheckListManager.Infrastructure.Logic.Email
{
    public class EmailService : EmailServiceBase, IEmailService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IQueueService _queueService;

        public EmailService(
            IQueueService queueService,
            ISettingService settingService,
            ICommonRepository commonRepository,
            IConfigurationService configurationService) : base(settingService, commonRepository, configurationService)
        {
            _queueService = queueService;
        }

        public async Task ProcessEmailMessagesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var messages =
                await _queueService.ReceiveObjectsAsync<EmailMessage>(QueueName.Email,
                    cancellationToken: cancellationToken);

            foreach (var message in messages)
            {
                await ProcessMessageAsync(message, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        private async Task ProcessMessageAsync(QueueMessage<EmailMessage> message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await SendInternal(message.Message);

                var deleteTask =
                    _queueService.DeleteMessageAsync(QueueName.Email, message.MessageHandle, cancellationToken);
                if (message.Message.TrailMessage != null)
                {
                    var recipients = message.Message.To.Concat(message.Message.Cc).ToArray();
                    message.Message.TrailMessage.Information.Recipients = recipients;
                    message.Message.TrailMessage.Information.Subject = message.Message.Subject;

                    var trailTask = _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = message.Message.Company,
                        User = message.Message.TrailMessage.User,
                        Action = TrailAction.EmailSent,
                        Date = DateTimeOffset.UtcNow,
                        CheckList = message.Message.TrailMessage.CheckList,
                        Information = message.Message.TrailMessage.Information.ToJson(new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.None
                        })
                    }, cancellationToken);

                    await Task.WhenAll(deleteTask, trailTask);
                }
                else
                {
                    await deleteTask;
                }
            }
            catch (Exception exception)
            {
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.EmailFailed,
                    Company = message.Message.Company,
                    Information = exception.Message,
                }, cancellationToken);

                Logger.Error(exception);
            }
        }
    }
}