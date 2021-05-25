using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CheckListManager.Trail.Data;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using NLog;

namespace CheckListManager.Trail.Logic.Services
{
	public class TrailService : ITrailService
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly IQueueService _queueService;
		private readonly ITrailRepository _trailRepository;
		private readonly IMapper _mapper;
		private readonly IEmailServiceBase _emailService;

		public TrailService(
			IQueueService queueService,
			ITrailRepository trailRepository,
			IMapper mapper,
			IEmailServiceBase emailService)
		{
			_queueService = queueService;
			_trailRepository = trailRepository;
			_mapper = mapper;
			_emailService = emailService;
		}

		public async Task ProcessMessagesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
            bool idle;
            do
            {
                idle = true;
                var messages = await _queueService.ReceiveObjectsAsync<TrailMessage>(QueueName.Trail, cancellationToken: cancellationToken);
                foreach (var message in messages)
                {
                    idle = false;
                    await ProcessMessageAsync(message, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }
            }
            while (!idle);
		}

		private async Task ProcessMessageAsync(QueueMessage<TrailMessage> message, CancellationToken cancellationToken)
		{
            Records record = null;
			try
			{
				record = _mapper.Map<Records>(message.Message);
				await _trailRepository.AddAsync(record);
				await _queueService.DeleteMessageAsync(QueueName.Trail, message.MessageHandle, cancellationToken);
			}
			catch (Exception exception)
			{
				await _emailService.LogErrorAsync(exception, "Error creating audit trail record");
				Logger.Error(record == null ? exception : new Exception($"Error creating audit trail record (action: {record.Action})", exception));
			}
		}
	}
}
