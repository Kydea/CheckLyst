namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListIteration : ICheckListGroup, IElementsBranch
    {
		int Index { get; set; }
	}
}