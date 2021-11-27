using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckListEditableItem : ICheckListItem, ICheckListValidityElement
    {
        string ModifiedBy { get; }
        DateTimeOffset? ModifiedOn { get; } 
    }
}