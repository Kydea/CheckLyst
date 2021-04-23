using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListInstanceLink : ICheckListEditableItem
    {
        string PrototypeName { get; set; }
        InstanceLinkActions Actions { get; set; }
    }
}