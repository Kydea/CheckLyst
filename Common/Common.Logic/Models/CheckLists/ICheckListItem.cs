using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListItem : ICheckListElement
	{
		string CssClass { get; }
		string Description { get; set; }
		[JsonIgnore]
		ICheckListGroup Group { get; set; }
		int Id { get; set; }
		[JsonIgnore]
		ICheckListIteration Iteration { get; set; }
		bool IterationItem { get; set; }
		bool IteratorModelItem { get; set; }
		bool NewLine { get; set; }
		[JsonIgnore]
		ICheckListPage Page { get; set; }
		[JsonIgnore]
		ICheckListItem Previous { get; set; }
		string TypeName { get; }
		ICheckListItem Clone(ICheckListGroup parent);
        int FontSize { get; set; }
        string Color { get; set; }
        void Remove();
        bool Removed { get; set; }
	}
}