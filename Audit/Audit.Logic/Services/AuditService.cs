using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CheckListManager.Audit.Data;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Audit;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using NLog;

namespace CheckListManager.Audit.Logic.Services
{
	public class AuditService : IAuditService
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly IQueueService _queueService;
		private readonly IAuditRepository _auditRepository;
		private readonly IMapper _mapper;

		public AuditService(IQueueService queueService, IAuditRepository auditRepository, IMapper mapper)
		{
			_queueService = queueService;
			_auditRepository = auditRepository;
			_mapper = mapper;
		}

		public async Task ProcessMessagesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var messages = await _queueService.ReceiveObjectsAsync<AuditMessage>(QueueName.Audit, cancellationToken: cancellationToken);

			foreach (var message in messages)
			{
				await ProcessMessageAsync(message, cancellationToken);

				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}
			}
		}

		private async Task ProcessMessageAsync(QueueMessage<AuditMessage> message, CancellationToken cancellationToken)
		{
			try
			{
				var record = _mapper.Map<Records>(message.Message);
				await _auditRepository.AddAsync(record);
				await _queueService.DeleteMessageAsync(QueueName.Audit, message.MessageHandle, cancellationToken);
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}
		}
	}
}
