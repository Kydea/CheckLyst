using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Instance : ICheckList
	{
		public Dictionary<string, ICheckListElement> Elements { get; set; } = new Dictionary<string, ICheckListElement>();
		public Dictionary<string, Expression> Expressions { get; set; }
		public ICheckListGroup Header { get; set; }
		public List<ICheckListPage> Pages { get; set; }

		public Formula Description { get; set; }


		public void AddItem(ICheckListItem item)
		{
			if (item.Id == 0)
				item.Id = ++_sequence;
			else
				_sequence = Math.Max(_sequence, item.Id);

			Items.Add(item.Id, item);
		}

		private int _sequence;

		[JsonIgnore]
		public Dictionary<int, ICheckListItem> Items { get; } = new Dictionary<int, ICheckListItem>(10);

		public void Setup()
		{
			Header.CheckList = this;
			Header.Setup();

			foreach (var page in Pages)
			{
				page.CheckList = this;
				page.Setup();
			}

			foreach (var expression in Expressions.Values)
				expression.Parse(this);
		}

		public void Update()
		{
			foreach (var expression in Expressions.Values)
			{
				expression.Parse(this);
				expression.Evaluate(this);
			}

			Description?.Update(Expressions, null);

			Header.Update();
			foreach (var page in Pages)
				page.Update();
		}

		public void UpdateReadOnly(string userCode, bool canEdit, string state)
		{
			Header.UpdateReadOnly(userCode, canEdit && state == InstanceState.Draft);
			foreach (var page in Pages)
				page.UpdateReadOnly(userCode, canEdit && state == InstanceState.Progress);
		}
	}
}
