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
    public class EmailService : EmailServiceInternal, IEmailService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IQueueService _queueService;
        private readonly ICommonRepository _commonRepository;

        public EmailService(
            IQueueService queueService,
            ISettingService settingService,
            ICommonRepository commonRepository) : base(settingService)
        {
            _queueService = queueService;
            _commonRepository = commonRepository;
        }

        public async Task ProcessEmailMessagesAsync(CancellationToken cancellationToken = default)
        {
            bool idle;
            do
            {
                idle = true;
                var messages = await _queueService.ReceiveObjectsAsync<EmailMessage>(QueueName.Email, cancellationToken: cancellationToken);

                foreach (var message in messages)
                {
                    idle = false;
                    await ProcessMessageAsync(message, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }
            }
            while (!idle);
        }

        private async Task ProcessMessageAsync(QueueMessage<EmailMessage> message, CancellationToken cancellationToken = default)
        {
            try
            {
                await SendInternal(message.Message);

                if (message.Message.TrailMessage != null)
                {
                    var recipients = message.Message.To.Concat(message.Message.Cc).ToArray();
                    message.Message.TrailMessage.Information.Recipients = recipients;
                    message.Message.TrailMessage.Information.Subject = message.Message.Subject;
                    await _commonRepository.AddTrailRecordAsync(new Records
                    {
                        Company = message.Message.Company,
                        User = message.Message.TrailMessage.User,
                        Action = TrailAction.EmailSent,
                        Date = DateTimeOffset.UtcNow,
                        CheckListCode = message.Message.TrailMessage.CheckListCode,
                        CheckListID = message.Message.TrailMessage.CheckListID,
                        Information = message.Message.TrailMessage.Information.ToJson(new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.None
                        })
                    });
                }

                await _queueService.DeleteMessageAsync(QueueName.Email, message.MessageHandle, cancellationToken);
            }
            catch (Exception exception)
            {
                await _commonRepository.AddTrailRecordAsync(new Records
                {
                    Action = TrailAction.EmailFailed,
                    User = message.Message.TrailMessage?.User,
                    Company = message.Message.Company,
                    CheckListCode = message.Message.TrailMessage?.CheckListCode,
                    CheckListID = message.Message.TrailMessage?.CheckListID,
                    Information = exception.Message,
                });

                Logger.Error(exception);
            }
        }
    }
}