using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListGroup : ICheckListEditableItem
    {
		List<ICheckListItem> Items { get; set; }
		List<ICheckListIteration> Iterations { get; }
		bool Iterator { get; set; }
        bool ShowAsTable { get; set; }
        int InstanceIterations { get; set; }
        int PrototypeIterations { get; set; }
        bool DisableAdd { set; } //for compatibility with version < 3.0
        bool DisableRemove { set; } //for compatibility with version < 3.0
        bool DisableMove { set; } //for compatibility with version < 3.0
        Conditions AddIterationCondition { get; set; }
        string AddIterationExpressionCode { get; set; }
        Conditions RemoveIterationCondition { get; set; }
        string RemoveIterationExpressionCode { get; set; }
        Conditions MoveIterationCondition { get; set; }
        string MoveIterationExpressionCode { get; set; }


        void CreateInstanceIterations();
    }
}