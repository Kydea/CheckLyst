using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Services;
using NLog;

namespace CheckListManager.Common.Logic.Email
{
    public class EmailServiceInternal
    {
        private readonly ISettingService _settingService;

        public EmailServiceInternal(ISettingService settingService)
        {
            _settingService = settingService;
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

            var smtpClient = new SmtpClient(await _settingService.SmtpHostAsync(message.Company), await _settingService.SmtpPortAsync(message.Company))
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
    
    public class EmailServiceBase : EmailServiceInternal, IEmailServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICommonRepository _commonRepository;
        private readonly IConfigurationService _configurationService;

        private readonly string _system;
        private readonly string _environment;

        public EmailServiceBase(
            ISettingService settingService,
            ICommonRepository commonRepository,
            IConfigurationService configurationService) : base(settingService)
        {
            _commonRepository = commonRepository;
            _configurationService = configurationService;

            _system = ConfigurationManager.AppSettings["System"];
            _environment = ConfigurationManager.AppSettings["Environment"];
        }

        private static string GetExceptionContent(Exception exception)
        {
            var sb = new StringBuilder();

            while (exception != null)
            {
                sb.AppendLine($"{exception.Message}<br />");
                sb.AppendLine($"{exception.StackTrace}<br />");

                if (exception.InnerException != null)
                {
                    sb.AppendLine("<br /><br />");
                    sb.AppendLine("---------------------");
                    sb.AppendLine("<br /><br />");
                }

                exception = exception.InnerException;
            }
            
            
            return sb.ToString();
        }

        private string GetSubject(string level, string subject)
            => $"{_system} / {_environment} - {level} - {subject}";

        private async Task<long?> GetCompanyId()
        {
            var companies = await _commonRepository.GetCompaniesAsync();
            return companies.FirstOrDefault(x => x.ID == 1)?.ID ?? companies.FirstOrDefault()?.ID;
        }

        public async Task LogInfoAsync(string subject, string content)
        {
            try
            {
                var companyId = await GetCompanyId();
                if (companyId == null) return;
                
                var message = new EmailMessage(
                    companyId.Value,
                    GetSubject("INFO", subject),
                    content,
                    _configurationService.NotificationsEmailAddress
                );
                
                await SendInternal(message);
            }
            catch (Exception e)
            {
                Logger.Warn(e);
            }
        }

        public async Task LogErrorAsync(Exception exception, string subject = null)
        {
            if (exception == null)
                return;
            
            try
            {
                var companyId = await GetCompanyId();
                if (companyId == null) return;
                
                var message = new EmailMessage(
                    companyId.Value,
                    GetSubject("CRITICAL", subject ?? "Error"),
                    GetExceptionContent(exception),
                    _configurationService.NotificationsEmailAddress
                );
                
                await SendInternal(message);
            }
            catch (Exception e)
            {
                Logger.Warn(e);
            }
        }
    }
}