using System;

namespace CheckListManager.Common.Logic.Api.Messages
{
	public class PagingResponse<T> : ResponseMessage<T>
	{
		public int Count { get; set; }
		public int PagesCount { get { return (int)Math.Ceiling(Count / (decimal)ItemsPerPage); } }
		public int CurrentPage { get; set; }
		public int ItemsPerPage { get; set; }
	}
}
