using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Iteration : Group, ICheckListIteration
	{
		public override string TypeName => "Iteration";
		public int Index { get; set; }
	}
}
