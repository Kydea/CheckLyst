namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListLink : ICheckListItem
	{
        string Url { get; set; }
	}
}