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
using CheckListManager.Prototypes.Data;
using CheckListManager.Common.Logic.Terminal.FTP;
using CheckListManager.Common.Data;
using CheckListManager.Prototypes.Logic.PDF;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Json;

namespace CheckListManager.Prototypes.Terminal
{
    public class FTPService : IFTPService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly CheckListJsonConverter _converter;
        private readonly IQueueService _queueService;
        private readonly ISettingService _settingService;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IStorageService _storageService;

        public FTPService(
            CheckListJsonConverter converter,
            IQueueService queueService,
            ISettingService settingService,
            IPrototypesRepository prototypesRepository,
            ICommonRepository commonRepository,
            IConfigurationService configurationService,
            IStorageService storageService
        )
        {
            _converter = converter;
            _queueService = queueService;
            _settingService = settingService;
            _prototypesRepository = prototypesRepository;
            _commonRepository = commonRepository;
            _configurationService = configurationService;
            _storageService = storageService;
        }

        public async Task ProcessFTPMessagesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var messages = await _queueService.ReceiveObjectsAsync<PrototypesFTPMessage>(QueueName.PrototypesFTP, cancellationToken: cancellationToken);

            foreach (var message in messages)
            {
                await ProcessMessageAsync(message, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        private async Task ProcessMessageAsync(QueueMessage<PrototypesFTPMessage> message, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var Settings = await Send(message.Message);
                await _queueService.DeleteMessageAsync(QueueName.PrototypesFTP, message.MessageHandle, cancellationToken);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.PrototypeExportCompleted,
                    Company = message.Message.Company,
                    CheckList = $"{message.Message.PrototypeName}.{message.Message.Version}",
                    Information = JsonConvert.SerializeObject(new
                    {
                        Exported = Settings != null,
                        File = message.Message.Path,
                        Server = Settings
                    })
                }, cancellationToken);
            }
            catch (Exception exception)
            {
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.PrototypeExportFailed,
                    Company = message.Message.Company,
                    CheckList = $"{message.Message.PrototypeName}.{message.Message.Version}",
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

        private async Task<FTPConnectionSettings> Send(PrototypesFTPMessage message)
        {
            ////we are not using company settings from v2.0.0
            //var SerializedSettings = await _settingService.PrototypesFtpConnectionAsync(message.Company);
            //if (string.IsNullOrWhiteSpace(SerializedSettings)) return;
            //Settings = JsonConvert.DeserializeObject<FTPConnectionSettings>(SerializedSettings);

            var PrototypeVersion = await _prototypesRepository.GetPrototypeVersionAsync(message.Company, message.Prototype, message.Version);

            FTPConnectionSettings Settings;
            var FTPServer = await _commonRepository.GetPrototypesFTPServerAsync(message.Company, PrototypeVersion.Prototypes.PrototypeType);
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

            var CheckList = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converter);

            var PDF = new PrototypePDF(
                Company.ID,
                Company.Name,
                Company.TimeZones.MicrosoftCode,
                _configurationService.PublicRepositoryUrl,
                PrototypeVersion,
                CheckList,
                _storageService,
                _configurationService
            );

            var MemoryStream = new MemoryStream();
            await PDF.GenerateAsync(MemoryStream);
            var ftpClient = new FTPClient();

            await ftpClient.ActionAsync(MemoryStream.ToArray(), Settings.Folder, $"{message.Path}{(message.State == VersionState.Obsolete ? "#O" : string.Empty)}", Settings, FTPClient.Methods.Upload);
            return Settings;
        }
    }
}