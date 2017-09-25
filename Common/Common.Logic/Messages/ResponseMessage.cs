using System.Collections.Generic;


namespace CheckListManager.Common.Logic.Messages
{
	public class ResponseMessage
	{
		public List<string> Messages { get; } = new List<string>();

		public static ResponseMessage<T> Create<T>(T data)
		{
			return new ResponseMessage<T> { Data = data };
		}
	}

	public class ResponseMessage<T> : ResponseMessage
	{
		public T Data { get; set; }
	}
}
