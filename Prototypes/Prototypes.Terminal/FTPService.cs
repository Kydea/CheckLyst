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
using CheckListManager.Common.Logic.Email;
using CheckListManager.Prototypes.Logic.PDF;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Json;
using System.Linq;

namespace CheckListManager.Prototypes.Terminal
{
    public class FTPService : IFTPService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JsonConverter[] _converters;
        private readonly IQueueService _queueService;
        private readonly ISettingService _settingService;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IStorageService _storageService;
        private readonly IEmailServiceBase _emailService;

        public FTPService(
            CustomJsonConverter[] converters,
            IQueueService queueService,
            ISettingService settingService,
            IPrototypesRepository prototypesRepository,
            ICommonRepository commonRepository,
            IConfigurationService configurationService,
            IStorageService storageService,
            IEmailServiceBase emailService
        )
        {
            _converters = converters.ToArray<JsonConverter>();
            _queueService = queueService;
            _settingService = settingService;
            _prototypesRepository = prototypesRepository;
            _commonRepository = commonRepository;
            _configurationService = configurationService;
            _storageService = storageService;
            _emailService = emailService;
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
                var Information = new
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
                };
                await _queueService.DeleteMessageAsync(QueueName.PrototypesFTP, message.MessageHandle, cancellationToken);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.PrototypeExportCompleted,
                    Company = message.Message.Company,
                    CheckListCode = $"{message.Message.PrototypeName}.{message.Message.Version}",
                    CheckListID = message.Message.Prototype,
                    Information = JsonConvert.SerializeObject(Information)
                }, cancellationToken);
            }
            catch (Exception exception)
            {
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.PrototypeExportFailed,
                    Company = message.Message.Company,
                    CheckListCode = $"{message.Message.PrototypeName}.{message.Message.Version}",
                    CheckListID = message.Message.Prototype,
                    Information = JsonConvert.SerializeObject(new
                    {
                        Exported = false,
                        File = message.Message.Path,
                        Error = exception.Message
                    })
                }, cancellationToken);

                Logger.Error(exception);

                await _emailService.LogErrorAsync(exception, "Error Sending FTP file");
            }
        }

        private async Task<FTPConnectionSettings> Send(PrototypesFTPMessage message)
        {
            var PrototypeVersion = await _prototypesRepository.GetPrototypeVersionAsync(message.Company, message.Prototype, message.Version);

            FTPConnectionSettings Settings;
            var FTPServer = await _commonRepository.GetPrototypesFTPServerAsync(message.Company, PrototypeVersion.Prototypes.PrototypeType);
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

            var CheckList = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converters);

            var PDF = new PrototypePDF(
                Company.ID,
                Company.Name,
                Company.TimeZone,
                _configurationService.PublicRepositoryUrl,
                PrototypeVersion,
                CheckList,
                _storageService,
                _configurationService
            );

            var MemoryStream = new MemoryStream();
            await PDF.GenerateAsync(MemoryStream);
            var ftpClient = FTPClientBuilder.Create(FTPServer.Protocol);

            await ftpClient.ActionAsync(MemoryStream.ToArray(), Settings.Folder, $"{message.Path}{(message.State == VersionState.Obsolete ? "#O" : string.Empty)}", Settings, FTPMethods.Upload);
            return Settings;
        }
    }
}