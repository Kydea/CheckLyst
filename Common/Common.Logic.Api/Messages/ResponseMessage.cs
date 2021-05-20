using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Api.Messages
{
	public class ResponseMessage
	{
		public ResponseMessage()
		{
			
		}

		public ResponseMessage(params string[] messages)
		{
			Messages.AddRange(messages);
		}

		public List<string> Messages { get; } = new List<string>();

		public static ResponseMessage<T> Create<T>(T data)
		{
			return new ResponseMessage<T> { Data = data };
		}
	}

	public class ResponseMessage<T> : ResponseMessage
	{
		public ResponseMessage()
		{
			
		}

		public ResponseMessage(params string[] messages) : base(messages)
		{
		}

		public T Data { get; set; }
	}
}
