using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListGroup : ICheckListItem
	{
		List<ICheckListItem> Items { get; set; }
		List<ICheckListIteration> Iterations { get; }
		bool Iterator { get; set; }
        bool ShowAsTable { get; set; }
        int InstanceIterations { get; set; }
        int PrototypeIterations { get; set; }
        bool DisableAdd { get; set; }
        bool DisableRemove { get; set; }
        bool DisableMove { get; set; }
    }
}