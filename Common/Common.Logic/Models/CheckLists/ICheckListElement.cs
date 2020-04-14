using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckListElement : ICheckListNode
    {
        string Code { get; set; }
        Conditions CountDownCondition { get; set; }
        string CountDownToCode { get; set; }
        int CountDownOffset { get; set; } //in seconds
        Conditions HiddenCondition { get; set; }
        string HiddenExpressionCode { get; set; }
        Conditions ReadOnlyCondition { get; set; }
        string ReadOnlyExpressionCode { get; set; }
        [JsonIgnore]
        ICheckListSignature Signature { get; }
        string SignatureCode { get; set; }
        bool Valid { get; set; }
        bool ReadOnly { get; }
        bool Hidden { get; set; }
        Conditions ValidCondition { get; set; }
        string ValidExpressionCode { get; set; }
        string ValidityMessage { get; set; }
        void Setup();
    }
}