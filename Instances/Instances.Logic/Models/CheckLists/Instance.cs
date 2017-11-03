using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Instance : ICheckList
	{
		public Dictionary<string, ICheckListElement> Elements { get; set; }
		public Dictionary<string, Expression> Expressions { get; set; }
		public ICheckListGroup Header { get; set; }
		public List<ICheckListPage> Pages { get; set; }

		public void AddItem(ICheckListItem item)
		{
			throw new NotImplementedException();
		}
	}
}
