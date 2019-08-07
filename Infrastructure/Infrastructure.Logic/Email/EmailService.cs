using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;
using NLog;

namespace CheckListManager.Infrastructure.Logic.Email
{
    public class EmailService : IEmailService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IQueueService _queueService;
        private readonly ISettingService _settingService;

        public EmailService(IQueueService queueService, ISettingService settingService)
        {
            _queueService = queueService;
            _settingService = settingService;
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
                await Send(message.Message);

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
                    Information = exception.Message
                }, cancellationToken);

                Logger.Error(exception);
            }
        }

        private async Task Send(EmailMessage message)
        {
            var body = new StringBuilder();

            body = body.Append("<h1>CheckList Manager - " + message.Subject + "</h1>");
            if (message.Content != null)
                body = body.Append($"<p>{message.Content}</p>");

            var sender = await _settingService.EmailSenderAsync(message.Company);
            var mailMessage = new MailMessage { From = new MailAddress(sender) };

            if (message.To != null)
                foreach (var email in message.To)
                    mailMessage.To.Add(email);

            if (message.Cc != null)
                foreach (var email in message.Cc)
                    mailMessage.CC.Add(email);

            mailMessage.Body = body.ToString();

            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "CheckLyst - " + message.Subject;


            var username = await _settingService.SmtpUsernameAsync(message.Company);

            var secure = !string.IsNullOrEmpty(username);

            var smtpClient = new SmtpClient(await _settingService.SmtpHostAsync(message.Company),
                await _settingService.SmtpPortAsync(message.Company))
            {
                UseDefaultCredentials = true,
                EnableSsl = secure
            };

            if (secure)
                smtpClient.Credentials = new System.Net.NetworkCredential
                {
                    UserName = username,
                    Password = await _settingService.SmtpPasswordAsync(message.Company)
                };

            smtpClient.Send(mailMessage);
        }
    }
}