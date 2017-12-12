using System.Collections.Generic;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface IElementsBranch : ICheckListNode
    {
        [JsonIgnore]
        Dictionary<string, ICheckListElement> Elements { get; }
    }
}
