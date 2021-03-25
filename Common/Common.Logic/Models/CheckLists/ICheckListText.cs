namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListText : ICheckListItem
	{
        int Height { get; set; }
        int Width { get; set; }
    }
}