using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Services;
using NLog;

namespace CheckListManager.Common.Logic.Email
{
    public class EmailServiceBase : IEmailServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISettingService _settingService;

        public EmailServiceBase(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task Send(EmailMessage message)
        {
            try
            {
                await SendInternal(message);
            }
            catch (Exception exception)
            {
                Logger.Warn(exception);
            }
        }
        
        protected async Task SendInternal(EmailMessage message)
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