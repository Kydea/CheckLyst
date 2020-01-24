using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        void CreateInstanceIterations();
        void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action);
        Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action);
    }
}