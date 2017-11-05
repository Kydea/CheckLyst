using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Prototype : ICheckList
	{
		public Dictionary<string, ICheckListElement> Elements { get; } = new Dictionary<string, ICheckListElement>();
		public Dictionary<string, Expression> Expressions { get; set; } = new Dictionary<string, Expression>();
		public ICheckListGroup Header { get; set; }
		public List<ICheckListPage> Pages { get; } = new List<ICheckListPage>();
		public void AddItem(ICheckListItem item)
		{
			throw new NotImplementedException();
		}
	}
}
