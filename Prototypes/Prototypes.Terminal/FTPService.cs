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
using CheckListManager.Prototypes.Data;
using CheckListManager.Common.Logic.Terminal.FTP;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Common.Data;
using CheckListManager.Prototypes.Logic.PDF;
using CheckListManager.Prototypes.Logic.Messages;
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

        public FTPService(CheckListJsonConverter converter, IQueueService queueService, ISettingService settingService, IPrototypesRepository prototypesRepository, ICommonRepository commonRepository, IConfigurationService configurationService)
		{
            _converter = converter;
            _queueService = queueService;
			_settingService = settingService;
            _prototypesRepository = prototypesRepository;
            _commonRepository = commonRepository;
            _configurationService = configurationService;
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
				await Send(message.Message);
				await _queueService.DeleteMessageAsync(QueueName.PrototypesFTP, message.MessageHandle, cancellationToken);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Action = TrailAction.PrototypeExportCompleted,
                    Company = message.Message.Company,
                    CheckList = $"{message.Message.PrototypeName}.{message.Message.Version}",
                    Information = JsonConvert.SerializeObject(new
                    {
                        File = message.Message.Path
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
                        File = message.Message.Path,
                        Error = exception.Message
                    })
				}, cancellationToken);

				Logger.Error(exception);
			}
		}

		private async Task Send(PrototypesFTPMessage message)
		{
            var SerializedSettings = await _settingService.PrototypesFtpConnectionAsync(message.Company);
            if (string.IsNullOrWhiteSpace(SerializedSettings)) return;

            var Company = await _commonRepository.GetCompanyAsync(message.Company);

            var PrototypeVersion = await _prototypesRepository.GetPrototypeVersionAsync(message.Company, message.Prototype, message.Version);
            var CheckList = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converter);

            var PDF = new PrototypePDF(Company.ID, Company.Name, Company.TimeZones.MicrosoftCode, _configurationService.PublicRepositoryUrl, PrototypeVersion, CheckList);

            var MemoryStream = new MemoryStream();
            PDF.Generate(MemoryStream);
            var Settings = JsonConvert.DeserializeObject<FTPConnectionSettings>(SerializedSettings);
            var ftpClient = new FTPClient();

            await ftpClient.ActionAsync(MemoryStream.ToArray(), $"{(string.IsNullOrEmpty(Settings.Folder) ? string.Empty : Settings.Folder + "/")}{message.Path.Replace('/', '-')}{(message.State == VersionState.Obsolete ? "#O" : string.Empty)}", Settings, FTPClient.Methods.Upload);
        }
    }
}