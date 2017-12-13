using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckListElement : ICheckListNode
    {
        string Code { get; set; }
        [JsonIgnore]
        Expression CountDownTo { get; }
        string CountDownToCode { get; set; }
        bool Hidden { get; }
        Conditions HiddenCondition { get; set; }
        string HiddenExpressionCode { get; set; }
        Conditions ReadOnlyCondition { get; set; }
        string ReadOnlyExpressionCode { get; set; }
        [JsonIgnore]
        ICheckListSignature Signature { get; }
        string SignatureCode { get; set; }
        bool Valid { get; }
        bool IsReadOnly { get; }
        Conditions ValidCondition { get; set; }
        string ValidExpressionCode { get; set; }
        string ValidityMessage { get; set; }

        void Setup();
        void Update();
        void UpdateReadOnly(string userCode, bool canEdit);
    }
}