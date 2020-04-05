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
        
        private Instance _instance;
        [JsonIgnore]
        public Instance Instance => _instance ?? (_instance = (Instance)CheckList);

		protected virtual Dictionary<string, ICheckListElement> GetBranchElements() => CheckList.Elements;

		public virtual void Setup()
		{
            if (!string.IsNullOrEmpty(Code))
				CheckList.Elements[Code] = this;
		}

        private HashSet<DependencyNodeKey.Processes> NeedToUpdate { get; } = new HashSet<DependencyNodeKey.Processes>();

        public void Update()
        {
            if (NeedToUpdate.Contains(DependencyNodeKey.Processes.Value))
            {
                UpdateValue();
                NeedToUpdate.Remove(DependencyNodeKey.Processes.Value);
            }
            if (NeedToUpdate.Contains(DependencyNodeKey.Processes.Validity))
            {
                UpdateValid();
                NeedToUpdate.Remove(DependencyNodeKey.Processes.Validity);
            }
            if (NeedToUpdate.Contains(DependencyNodeKey.Processes.Hidden))
            {
                UpdateHidden();
                NeedToUpdate.Remove(DependencyNodeKey.Processes.Hidden);
            }
            if (NeedToUpdate.Contains(DependencyNodeKey.Processes.Other))
            {
                UpdateCountDownTo();
                UpdateOther();
                NeedToUpdate.Remove(DependencyNodeKey.Processes.Other);
            }
        }

        #region Properties

        public string Code { get; set; }

        public ICheckList CheckList { get; set; }

        public ICheckListSignature Signature => GetSignature(SignatureCode);
        public string SignatureCode { get; set; }
        public virtual string GetSignatureInformation() => Signature != null && Signature.Signed ? string.Format("by {0} on {1:yyyy-MM-dd HH:mm}", Signature.SignedBy, Signature.SignedOn) : null;

        protected abstract void UpdateValue();

        public virtual bool Valid { get; set; }
        public Conditions ValidCondition { get; set; }
        public string ValidExpressionCode { get; set; }
        public virtual string ValidityMessage { get; set; }
        public virtual bool IsValidByDefault() => true;
        protected virtual void UpdateValid()
        {
            var ExpressionValue = CheckList?.GetExpressionValue(ValidExpressionCode, this)?.Value as bool? == true;
            Valid = 
                Hidden ||
                ValidCondition == Conditions.Default && IsValidByDefault() ||
                ValidCondition == Conditions.Expression && ExpressionValue ||
                ValidCondition == Conditions.ExpressionOrDefault && (ExpressionValue || IsValidByDefault()) ||
                ValidCondition == Conditions.ExpressionAndDefault && ExpressionValue && IsValidByDefault() ||
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

        public bool Hidden { get; set; }
        public Conditions HiddenCondition { get; set; }
        public string HiddenExpressionCode { get; set; }
        protected virtual void UpdateHidden()
        {
            Hidden =
                HiddenCondition == Conditions.Expression && CheckList?.GetExpressionValue(HiddenExpressionCode, this)?.Value as bool? == true
                || HiddenCondition == Conditions.True;
        }

        protected abstract void UpdateOther();

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
            destination.MarkAllUpdateFlags();
        }

        #endregion

        #region dependency

        private Expression GetExpression(string ExpressionCode, DependencyNodeKey DependencyNodeKey, DependencyNodeKey.Processes Process)
        {
            if (string.IsNullOrEmpty(ExpressionCode))
                throw new Exception($"The {DependencyNodeKey} reference is missing");
            if (!CheckList.Expressions.TryGetValue(ExpressionCode, out var Expression))
                throw new Exception($"The {DependencyNodeKey} references to the inexistent expression {ExpressionCode}");
            DependingNodes.Add(Expression.DependencyNodeKey.Clone(Process, null));
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
            DependingNodes.Add(Signature.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "property Signed"));
            return Signature;
        }

        public virtual void ClearDependencies()
        {
            DependentNodes.Clear();
            DependentNodes.Clear();
        }

        public void MarkAllUpdateFlags()
        {
            NeedToUpdate.Add(DependencyNodeKey.Processes.Value);
            NeedToUpdate.Add(DependencyNodeKey.Processes.Validity);
            NeedToUpdate.Add(DependencyNodeKey.Processes.Hidden);
            NeedToUpdate.Add(DependencyNodeKey.Processes.Other);
        }

        public virtual void BuildDependencies()
        {
            MarkAllUpdateFlags();

            if (!string.IsNullOrEmpty(SignatureCode))
            {
                DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "property signature");
                GetSignature(SignatureCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);
            }

            if (CountDownCondition == Conditions.Expression)
            {
                DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "property countdown");
                GetExpression(CountDownToCode, DependencyNodeKey, DependencyNodeKey.Processes.Other).DependentNodes.Add(DependencyNodeKey);
            }

            if (CountDownCondition == Conditions.Signature)
            {
                DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "property countdown");
                GetSignature(CountDownToCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);
            }

            if (HiddenCondition == Conditions.Expression)
            {
                DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Hidden, "property hidden");
                GetExpression(HiddenExpressionCode, DependencyNodeKey, DependencyNodeKey.Processes.Hidden).DependentNodes.Add(DependencyNodeKey);
            }

            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
            {
                DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.ReadOnly, "property read-only");
                GetExpression(ReadOnlyExpressionCode, DependencyNodeKey, DependencyNodeKey.Processes.ReadOnly).DependentNodes.Add(DependencyNodeKey);
            }

            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
            {
                DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Validity, "validity");
                GetExpression(ValidExpressionCode, DependencyNodeKey, DependencyNodeKey.Processes.Validity).DependentNodes.Add(DependencyNodeKey);
            }
        }

        public virtual DependencyNodeKey DependencyNodeKey => string.IsNullOrEmpty(Code) ? null : new DependencyNodeKey("element", Code);
        public HashSet<DependencyNodeKey> DependentNodes { get; } = new HashSet<DependencyNodeKey>();
        public HashSet<DependencyNodeKey> DependingNodes { get; } = new HashSet<DependencyNodeKey>();

        private HashSet<DependencyNodeKey.Processes> DependencyChainProcesses = new HashSet<DependencyNodeKey.Processes>();
        protected virtual bool OnTraverseDependencyChain(ICheckList CheckList, Stack<string> DependencyChain, DependencyNodeKey.Processes Process, Func<IDependencyNode, bool> DependencyChainAction)
        {
            var Result = true;

            if (
                Process == DependencyNodeKey.Processes.Validity 
                && !TraverseDependencyChain(CheckList, DependencyChain, DependencyNodeKey.Processes.Hidden, DependencyChainAction)
            )
                Result = false;

            foreach (var DependencyNodeKey in DependingNodes.Where(n => n.Process == Process))
                if (!DependencyNodeKey.GetNode(CheckList).TraverseDependencyChain(CheckList, DependencyChain, DependencyNodeKey.Process, DependencyChainAction))
                    Result = false;

            return Result;
        }

        public bool TraverseDependencyChain(ICheckList CheckList, Stack<string> DependencyChain, DependencyNodeKey.Processes Process, Func<IDependencyNode, bool> DependencyChainAction)
        {
            if (!NeedToUpdate.Contains(Process)) return true;

            DependencyChain.Push(DependencyNodeKey.Clone(Process, null).ToString());
            if (DependencyChainProcesses.Contains(Process))
                throw new Exception($"Dependency cycle found: {string.Join(" > ", DependencyChain.Reverse())}");
            DependencyChainProcesses.Add(Process);

            var Result = OnTraverseDependencyChain(CheckList, DependencyChain, Process, DependencyChainAction);

            if (Result)
                if (this is Item Item && Item.IteratorModelItem)
                {
                    var IterationItems = Item.FindIterationItems();
                    foreach (var IterationItem in IterationItems)
                    {
                        IterationItem.NeedToUpdate.Add(Process);
                        IterationItem.Update();
                    }
                    NeedToUpdate.Remove(Process);
                }
                else
                    Update();

            DependencyChain.Pop();
            DependencyChainProcesses.Remove(Process);
            return Result;
        }

        protected virtual bool OnMarkNodeAsChanged(ICheckList CheckList, List<IDependencyNode> Roots, DependencyNodeKey.Processes Process)
        {
            var Marked = false;

            if (Process == DependencyNodeKey.Processes.Hidden)
            {
                MarkNodeAsChanged(CheckList, Roots, DependencyNodeKey.Processes.Validity);
                Marked = true;
            }
            if (DependentNodes.Any())
            {
                foreach (var DependencyNodeKey in DependentNodes.Where(n => n.Process == Process))
                    DependencyNodeKey.GetNode(CheckList).MarkNodeAsChanged(CheckList, Roots, Process);
                Marked = true;
            }

            return Marked;
        }

        public void MarkNodeAsChanged(ICheckList CheckList, List<IDependencyNode> Roots, DependencyNodeKey.Processes Process)
        {
            if (Process == DependencyNodeKey.Processes.ReadOnly || NeedToUpdate.Contains(Process)) return;
            NeedToUpdate.Add(Process);

            if (!OnMarkNodeAsChanged(CheckList, Roots, Process))
                Roots.Add(this);
        }

        #endregion
    }
}
