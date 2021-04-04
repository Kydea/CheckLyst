namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListLink : ICheckListItem
	{
        string Url { get; set; }
        string ElementType { get; set; }
        string ElementCode { get; set; }
    }
}