namespace CheckListManager.Common.Logic.Api.Messages
{
	public class PagingResponse<T> : ResponseMessage<T>
	{
		public int Count { get; set; }
		public int PageCount { get; set; }
		public int CurrentPage { get; set; }
		public int ItemsPerPage { get; set; }
	}
}
