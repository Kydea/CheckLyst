using System.ComponentModel.DataAnnotations;

namespace CheckListManager.Common.Logic.Api.Messages
{
	public static class Message
	{
		public static Message<T> Create<T>(T data)
		{
			return new Message<T> { Data = data };
		}
	}

	public class Message<T>
	{
		[Required]
		public T Data { get; set; }
	}
}
