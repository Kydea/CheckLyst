using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using CheckListManager.Instances.Logic.Exceptions;
using Newtonsoft.Json;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using System.Linq;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public abstract class Element : ICheckListElement
    {
        private ICheckListSignature GetSignature(string SignatureCode)
        {
            var BranchElements = GetBranchElements();
            if (SignatureCode == null || BranchElements == null) return null;
            if (!BranchElements.ContainsKey(SignatureCode))
                throw new InstanceException((string.IsNullOrEmpty(Code) ? "An element" : $"The element {Code}") + $" references to the inexistent signature {SignatureCode}");
            if (!(BranchElements[SignatureCode] is ICheckListSignature Signature))
                throw new InstanceException((string.IsNullOrEmpty(Code) ? "An element" : $"The element {Code}") + $" references to a signature with code {SignatureCode} that is not a signature");
            return Signature;
        }

        [JsonIgnore]
        public Instance Instance { get; private set; }

		protected virtual Dictionary<string, ICheckListElement> GetBranchElements() => CheckList.Elements;

		public virtual void Setup()
		{
            Instance = (Instance)CheckList;
            if (!string.IsNullOrEmpty(Code))
				CheckList.Elements[Code] = this;
		}

        protected bool Updated { get; set; } = true;

        public void Update(string DecimalSeparator, string TimeZone)
        {
            if (Updated) return;

            OnUpdate(DecimalSeparator, TimeZone);
            UpdateValid();
            UpdateCountDownTo();
            UpdateHidden();
            Updated = true;
        }

        protected abstract void OnUpdate(string DecimalSeparator, string TimeZone);

        #region Properties

        public string Code { get; set; }

        public ICheckList CheckList { get; set; }

        public ICheckListSignature Signature => GetSignature(SignatureCode);
        public string SignatureCode { get; set; }
        public virtual string GetSignatureInformation() => Signature != null && Signature.Signed ? string.Format("by {0} on {1:yyyy-MM-dd HH:mm}", Signature.SignedBy, Signature.SignedOn) : null;

        public virtual bool Valid { get; set; }
        public Conditions ValidCondition { get; set; }
        public string ValidExpressionCode { get; set; }
        public virtual string ValidityMessage { get; set; }
        public virtual bool IsValidByDefault() => true;
        private void UpdateValid()
        {
            if (Hidden)
            {
                Valid = true;
                return;
            }

            var expressionValue = CheckList?.GetExpressionValue(ValidExpressionCode, this)?.Value as bool? == true;
            Valid = 
                ValidCondition == Conditions.Default && IsValidByDefault() ||
                ValidCondition == Conditions.Expression && expressionValue ||
                ValidCondition == Conditions.ExpressionOrDefault && (expressionValue || IsValidByDefault()) ||
                ValidCondition == Conditions.ExpressionAndDefault && expressionValue && IsValidByDefault() ||
                ValidCondition == Conditions.True;
        }

        public bool ReadOnly { get; protected set; }
        public Conditions ReadOnlyCondition { get; set; }
        public string ReadOnlyExpressionCode { get; set; }
        protected virtual bool IsReadOnlyByDefault() => false;
        public virtual void UpdateReadOnly(string userCode, bool readOnlyInstance)
		{
			if (readOnlyInstance)
			{
				ReadOnly = true;
				return;
			}

            var expressionValue = CheckList?.GetExpressionValue(ReadOnlyExpressionCode, this)?.Value as bool? == true;
			ReadOnly = (Signature?.Signed ?? false) ||
				ReadOnlyCondition == Conditions.Default && IsReadOnlyByDefault() ||
				ReadOnlyCondition == Conditions.Expression && expressionValue ||
				ReadOnlyCondition == Conditions.ExpressionOrDefault && (expressionValue || IsReadOnlyByDefault()) ||
				ReadOnlyCondition == Conditions.ExpressionAndDefault && expressionValue && IsReadOnlyByDefault() ||
				ReadOnlyCondition == Conditions.True;
		}

        public Conditions CountDownCondition { get; set; }
        public DateTimeOffset? CountDownToValue { get; set; }
        public string CountDownToCode { get; set; }
        public int CountDownOffset { get; set; }
        private void UpdateCountDownTo()
        {
            DateTimeOffset? Value;
            if (string.IsNullOrEmpty(CountDownToCode))
                Value = null;
            else if (CountDownCondition == Conditions.Signature)
            {
                var Signature = GetSignature(CountDownToCode);
                Value = Signature.Signed ? Signature.SignedOn : default(DateTimeOffset?);
            }
            else
                Value = CheckList?.GetExpressionValue(CountDownToCode, this)?.Value as DateTimeOffset?;

            if (Value.HasValue) Value = Value.Value.AddSeconds(CountDownOffset);
            CountDownToValue = Value;
        }

        public virtual bool Hidden { get; set; }
        public Conditions HiddenCondition { get; set; }
        public string HiddenExpressionCode { get; set; }
        private void UpdateHidden()
        {
            Hidden =
                HiddenCondition == Conditions.Expression && CheckList?.GetExpressionValue(HiddenExpressionCode, this)?.Value as bool? == true
                || HiddenCondition == Conditions.True;
        }

        public static void Copy(Element source, Element destination)
		{
			destination.CheckList = source.CheckList;
			destination.Code = source.Code;
            destination.CountDownToCode = source.CountDownToCode;
            destination.ReadOnlyExpressionCode = source.ReadOnlyExpressionCode;
			destination.ReadOnlyCondition = source.ReadOnlyCondition;
			destination.HiddenExpressionCode = source.HiddenExpressionCode;
			destination.HiddenCondition = source.HiddenCondition;
			destination.ValidExpressionCode = source.ValidExpressionCode;
			destination.ValidCondition = source.ValidCondition;
			destination.ValidityMessage = source.ValidityMessage;
			destination.SignatureCode = source.SignatureCode;
        }

        #endregion

        #region dependency

        private Expression GetExpression(string ExpressionCode, DependencyNodeKey DependencyNodeKey)
        {
            if (ExpressionCode == null) return null;
            if (string.IsNullOrEmpty(ExpressionCode))
                throw new Exception($"The {DependencyNodeKey} reference is missing");
            if (!CheckList.Expressions.TryGetValue(ExpressionCode, out var Expression))
                throw new Exception($"The {DependencyNodeKey} references to the inexistent expression {ExpressionCode}");

            DependingNodes.Add(Expression.DependencyNodeKey);
            return Expression;
        }

        private Signature GetSignature(string SignatureCode, DependencyNodeKey DependencyNodeKey)
        {
            var BranchElements = GetBranchElements();
            if (string.IsNullOrEmpty(SignatureCode))
                throw new Exception($"The {DependencyNodeKey} reference is missing");
            if (!BranchElements.ContainsKey(SignatureCode))
                throw new Exception($"The {DependencyNodeKey} references to the inexistent signature {SignatureCode}");
            if (!(BranchElements[SignatureCode] is Signature Signature))
                throw new Exception($"The {DependencyNodeKey} references to a signature with code {SignatureCode} but it is not a signature");
            return Signature;
        }

        public virtual void ClearDependencies()
        {
            DependentNodes.Clear();
            DependentNodes.Clear();
        }

        public virtual void BuildDependencies()
        {
            var BranchElements = GetBranchElements();
            Updated = false;

            var DependencyNodeKey = this.DependencyNodeKey.Clone("signature");
            if (!string.IsNullOrEmpty(SignatureCode))
                GetSignature(SignatureCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("countdown");
            if (CountDownCondition == Conditions.Expression)
                GetExpression(CountDownToCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);

            if (CountDownCondition == Conditions.Signature)
                GetSignature(CountDownToCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("hidden");
            if (HiddenCondition == Conditions.Expression)
                GetExpression(HiddenExpressionCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("read-only");
            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
                GetExpression(ReadOnlyExpressionCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("validity");
            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                GetExpression(ValidExpressionCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);
        }

        public virtual DependencyNodeKey DependencyNodeKey => string.IsNullOrEmpty(Code) ? null : new DependencyNodeKey("element", Code);
        public HashSet<DependencyNodeKey> DependentNodes { get; } = new HashSet<DependencyNodeKey>();
        public HashSet<DependencyNodeKey> DependingNodes { get; } = new HashSet<DependencyNodeKey>();

        private string DependencyChainKey { get; set; }

        protected virtual IEnumerable<DependencyNodeKey> GetDependingNodes() => DependingNodes;
        public bool TraverseDependencyChain(ICheckList checkList, Stack<IDependencyNode> DependencyChain, string DependencyChainKey, Func<IDependencyNode, bool> DependencyChainAction)
        {
            if (Updated) return true;

            DependencyChain.Push(this);
            if (this.DependencyChainKey == DependencyChainKey)
                throw new Exception($"Dependency cycle found: {string.Join(" > ", DependencyChain.Reverse().Select(e => e.DependencyNodeKey.Description))}");
            this.DependencyChainKey = DependencyChainKey;

            var Result = true;

            var DependingNodes = GetDependingNodes();
            foreach (var DependencyNodeKey in DependingNodes)
                if (!DependencyNodeKey.GetNode(checkList).TraverseDependencyChain(checkList, DependencyChain, DependencyChainKey, DependencyChainAction))
                    Result = false;

            if (Result)
                if (this is Item Item && Item.IteratorModelItem)
                {
                    var IterationItems = Item.FindIterationItems();
                    foreach (var IterationItem in IterationItems)
                    {
                        IterationItem.Updated = false;
                        IterationItem.Update(Instance.DecimalSeparator, Instance.TimeZone);
                    }
                    Updated = true;
                }
                else
                    Update(Instance.DecimalSeparator, Instance.TimeZone);

            DependencyChain.Pop();
            this.DependencyChainKey = null;
            return Result;
        }

        protected virtual IEnumerable<DependencyNodeKey> GetDependentNodes() => DependentNodes;
        public void MarkNodeAsChanged(ICheckList checkList, List<IDependencyNode> roots)
        {
            if (!Updated) return;
            Updated = false;
            var DependentNodes = GetDependentNodes();
            if (DependentNodes.Any())
                foreach (var DependencyNodeKey in DependentNodes)
                    DependencyNodeKey.GetNode(checkList).MarkNodeAsChanged(checkList, roots);
            else
                roots.Add(this);
        }

        #endregion
    }
}
