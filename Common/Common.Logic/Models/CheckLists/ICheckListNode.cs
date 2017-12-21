using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckListNode
    {
        [JsonIgnore]
        ICheckList CheckList { get; set; }
    }
}
