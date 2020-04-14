using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListPage : ICheckListElement
	{
		string Description { get; set; }
		List<ICheckListGroup> Groups { get; set; }
        void CreateInstanceIterations();
    }
}