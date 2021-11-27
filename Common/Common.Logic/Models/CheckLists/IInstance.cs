using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface IInstance : ICheckList
    {
        DateTimeOffset CreatedOn { get; }
        DateTimeOffset? InProgressOn { get; }
    }
}
