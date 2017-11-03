namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListIteration : ICheckListGroup
	{
		int Index { get; set; }
	}
}