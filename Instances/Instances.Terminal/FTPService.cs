using System;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.FTP;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using NLog;
using Newtonsoft.Json;
using System.IO;
using CheckListManager.Common.Logic.Terminal.FTP;
using CheckListManager.Instances.Data;
using CheckListManager.Instances.Logic.Services;
using CheckListManager.Instances.Logic.PDF;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Email;

namespace CheckListManager.Instances.Terminal
{
    public class FTPService : IFTPService
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly IQueueService _queueService;
		private readonly ISettingService _settingService;
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly ICommonRepository _commonRepository;
        private readonly IInstancesRepository _instancesRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IStorageService _storageService;
        private readonly IEmailServiceBase _emailService;

        public FTPService(
            IQueueService queueService,
            ISettingService settingService,
            IInstanceManagerService instanceManagerService,
            ICommonRepository commonRepository,
            IInstancesRepository instancesRepository,
            IConfigurationService configurationService,
            IStorageService storageService,
            IEmailServiceBase emailService)
		{
			_queueService = queueService;
			_settingService = settingService;
            _instanceManagerService = instanceManagerService;
            _commonRepository = commonRepository;
            _instancesRepository = instancesRepository;
            _configurationService = configurationService;
            _storageService = storageService;
            _emailService = emailService;
        }

        public async Task ProcessFTPMessagesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
            var messages = await _queueService.ReceiveObjectsAsync<InstancesFTPMessage>(QueueName.InstancesFTP, cancellationToken: cancellationToken);
            foreach (var message in messages)
            {
                await ProcessMessageAsync(message, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        private async Task ProcessMessageAsync(QueueMessage<InstancesFTPMessage> message,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var Settings = await Send(message.Message);
                await _queueService.DeleteMessageAsync(QueueName.InstancesFTP, message.MessageHandle, cancellationToken);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.InstanceExportCompleted,
                    Company = message.Message.Company,
                    CheckListCode = message.Message.Code,
                    CheckListID = message.Message.ID,
                    Information = JsonConvert.SerializeObject(
                        new
                        {
                            Exported = Settings != null,
                            File = message.Message.Path,
                            Server = Settings == null ? null : new
                            {
                                Settings.Host,
                                Settings.Port,
                                Settings.Folder,
                                Settings.EnableSSL,
                                Settings.UsePassive,
                                Settings.UserName,
                            }
                        }
                    )
                }, cancellationToken);
            }
            catch (Exception exception)
			{
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.InstanceExportFailed,
                    Company = message.Message.Company,
                    CheckListCode = message.Message.Code,
                    CheckListID = message.Message.ID,
                    Information = JsonConvert.SerializeObject(new
                    {
                        Exported = false,
                        File = message.Message.Path,
                        Error = exception.Message
                    })
                }, cancellationToken);
                await _emailService.LogErrorAsync(exception, "Error sending FTP file");

                Logger.Error(exception);
			}
		}

		private async Task<FTPConnectionSettings> Send(InstancesFTPMessage message)
		{
            ////we are not using company settings from v2.0.0
            //var SerializedSettings = await _settingService.InstancesFtpConnectionAsync(message.Company);
            //if (string.IsNullOrWhiteSpace(SerializedSettings)) return;
            //Settings = JsonConvert.DeserializeObject<FTPConnectionSettings>(SerializedSettings);

            string code = await _instancesRepository.GetCodeAsync(message.Company, message.ID);
            var cacheObject = await _instanceManagerService.GetCacheObjectForDownloadAsync(message.Company, code, true);
            FTPConnectionSettings Settings;
            var FTPServer = await _commonRepository.GetInstancesFTPServerAsync(message.Company, cacheObject.PrototypeType);
            if (FTPServer == null)
                return null;

            Settings = new FTPConnectionSettings
            {
                Protocol = FTPServer.Protocol,
                Host = FTPServer.Host,
                Port = FTPServer.Port,
                Folder = FTPServer.Folder,
                EnableSSL = FTPServer.SSL,
                UsePassive = FTPServer.Passive,
                UserName = FTPServer.UserName,
                Password = FTPServer.Password,
                Key = FTPServer.Key
            };

            var Company = await _commonRepository.GetCompanyAsync(message.Company);

            var PDF = new InstancePDF(Company.ID, Company.Name, Company.TimeZone, _configurationService.PublicRepositoryUrl, cacheObject, _instancesRepository, _storageService, _configurationService);
            var MemoryStream = new MemoryStream();
            await PDF.GenerateAsync(MemoryStream);

            var ftpClient = FTPClientBuilder.Create(FTPServer.Protocol);

            await ftpClient.ActionAsync(MemoryStream.ToArray(), Settings.Folder, message.Path, Settings, FTPMethods.Upload);
            return Settings;
        }
    }
}