namespace CheckListManager.Common.Logic.Messages
{
	public class PagingResponse<T> : Message<T>
	{
		public int Count { get; set; }
		public int PageCount { get; set; }
		public int CurrentPage { get; set; }
		public int ItemsPerPage { get; set; }
	}
}
