using System;
using System.Net.Mail;
using System.Text;
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

        public FTPService(IQueueService queueService, ISettingService settingService, IInstanceManagerService instanceManagerService, ICommonRepository commonRepository, IInstancesRepository instancesRepository, IConfigurationService configurationService, IStorageService storageService)
		{
			_queueService = queueService;
			_settingService = settingService;
            _instanceManagerService = instanceManagerService;
            _commonRepository = commonRepository;
            _instancesRepository = instancesRepository;
            _configurationService = configurationService;
            _storageService = storageService;
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
                    CheckList = message.Message.Code,
                    Information = JsonConvert.SerializeObject(
                        new
                        {
                            Exported = Settings != null,
                            File = message.Message.Path,
                            Server = Settings 
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
                    CheckList = message.Message.Code,
                    Information = JsonConvert.SerializeObject(new
                    {
                        Exported = false,
                        File = message.Message.Path,
                        Error = exception.Message
                    })
                }, cancellationToken);

                Logger.Error(exception);
			}
		}

		private async Task<FTPConnectionSettings> Send(InstancesFTPMessage message)
		{
            ////we are not using company settings from v2.0.0
            //var SerializedSettings = await _settingService.InstancesFtpConnectionAsync(message.Company);
            //if (string.IsNullOrWhiteSpace(SerializedSettings)) return;
            //Settings = JsonConvert.DeserializeObject<FTPConnectionSettings>(SerializedSettings);

            var cacheObject = await _instanceManagerService.GetAsync(message.Company, message.ID);
            cacheObject.Instance.Update(await _settingService.DecimalSeparatorAsync(cacheObject.CompanyId), (await _settingService.TimeZoneAsync(cacheObject.CompanyId)).MicrosoftCode, cacheObject);
            cacheObject.Instance.UpdateReadOnly(null, cacheObject.InstanceState);

            FTPConnectionSettings Settings;
            var FTPServer = await _commonRepository.GetInstancesFTPServerAsync(message.Company, cacheObject.PrototypeType);
            if (FTPServer == null)
                return null;

            Settings = new FTPConnectionSettings
            {
                Host = FTPServer.Host,
                Port = FTPServer.Port,
                Folder = FTPServer.Folder,
                EnableSSL = FTPServer.SSL,
                UsePassive = FTPServer.Passive,
                UserName = FTPServer.UserName,
                Password = FTPServer.Password
            };

            var Company = await _commonRepository.GetCompanyAsync(message.Company);

            var PDF = new InstancePDF(Company.ID, Company.Name, Company.TimeZones.MicrosoftCode, _configurationService.PublicRepositoryUrl, cacheObject, _instancesRepository, _storageService, _configurationService);
            var MemoryStream = new MemoryStream();
            await PDF.GenerateAsync(MemoryStream);

            var ftpClient = new FTPClient();

            await ftpClient.ActionAsync(MemoryStream.ToArray(), Settings.Folder, message.Path, Settings, FTPClient.Methods.Upload);
            return Settings;
        }
    }
}