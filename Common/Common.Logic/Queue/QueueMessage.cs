namespace CheckListManager.Common.Logic.Queue
{
	public class QueueMessage<T>
	{
		public QueueMessage()
		{
			
		}

		public QueueMessage(string messageHandle, T message)
		{
			MessageHandle = messageHandle;
			Message = message;
		}

		public string MessageHandle { get; set; }
		public T Message { get; set; }
	}

	public sealed class QueueMessage : QueueMessage<string>
	{
		public QueueMessage()
		{
			
		}

		public QueueMessage(string messageHandle, string message) : base(messageHandle, message)
		{
		}
	}
}
