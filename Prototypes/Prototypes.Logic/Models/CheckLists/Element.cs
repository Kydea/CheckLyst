using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public abstract class Element : ICheckListElement
    {
        public ICheckList CheckList { get; set; }
        public string Code { get; set; }
        public Conditions CountDownCondition { get; set; }
        public string CountDownToCode { get; set; }
        public int CountDownOffset { get; set; }
        public Conditions HiddenCondition { get; set; }
        public string HiddenExpressionCode { get; set; }
        public Conditions ReadOnlyCondition { get; set; }
        public string ReadOnlyExpressionCode { get; set; }
        public string SignatureCode { get; set; }
        public bool ReadOnly { get; }
        public Conditions ValidCondition { get; set; }
        public string ValidExpressionCode { get; set; }
        public string ValidityMessage { get; set; }
        [JsonIgnore]
        public bool Hidden { get; set; }
        [JsonIgnore]
        public ICheckListSignature Signature => null;
        [JsonIgnore]
        public bool Valid { get; set; }

        public virtual void Setup()
        {
            if (!string.IsNullOrEmpty(Code))
            {
                if (CheckList.Elements.ContainsKey(Code))
                    throw new Exception($"Duplicated code {Code}");
                CheckList.Elements[Code] = this;
            }
            ClearDependencies();
        }

        protected virtual Dictionary<string, ICheckListElement> GetBranchElements()
        {
            return CheckList.Elements;
        }

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

        public void ClearDependencies()
        {
            DependingNodes.Clear();
            DependentNodes.Clear();
        }

        public virtual void BuildDependencies()
        {
            DependencyNodeKey DependencyNodeKey;
            if (!string.IsNullOrEmpty(SignatureCode))
            {
                DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "property signature");
                GetSignature(SignatureCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);
            }

            if (CountDownCondition == Conditions.Expression)
            {
                DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "property countdown");
                GetExpression(CountDownToCode, DependencyNodeKey, DependencyNodeKey.Processes.Other).DependentNodes.Add(DependencyNodeKey);
            }

            if (CountDownCondition == Conditions.Signature)
            {
                DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "property countdown");
                GetSignature(CountDownToCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);
            }

            if (HiddenCondition == Conditions.Expression)
            {
                DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Hidden, "property hidden");
                GetExpression(HiddenExpressionCode, DependencyNodeKey, DependencyNodeKey.Processes.Hidden).DependentNodes.Add(DependencyNodeKey);
            }

            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
            {
                DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.ReadOnly, "property read-only");
                GetExpression(ReadOnlyExpressionCode, DependencyNodeKey, DependencyNodeKey.Processes.ReadOnly).DependentNodes.Add(DependencyNodeKey);
            }

            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
            {
                DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Validity, "validity");
                GetExpression(ValidExpressionCode, DependencyNodeKey, DependencyNodeKey.Processes.Validity).DependentNodes.Add(DependencyNodeKey);
            }
        }

        private void CheckExpression(string ExpressionCode, params DataType[] DataType)
        {
            var Expression = CheckList.Expressions[ExpressionCode];
            if (!DataType.Any(dt => dt == (Expression.InnerDataType ?? Expression.DataType)))
                throw new Exception($"The {DependencyNodeKey} references to the expression {ExpressionCode} which value is not {string.Join(" or ", DataType.Select(dt => dt.TypeName))}");
        }

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

        public bool TraverseDependencyChain(ICheckList checkList, Stack<string> DependencyChain, DependencyNodeKey.Processes Process, Func<IDependencyNode, bool> DependencyChainAction)
        {
            //look for cycles
            if (Process == DependencyNodeKey.Processes.Other || Process == DependencyNodeKey.Processes.ReadOnly) return true; //Other and ReadOnly do not have a dependency chain

            DependencyChain.Push(DependencyNodeKey.Clone(Process, null).ToString());
            if (DependencyChainProcesses.Contains(Process))
                throw new Exception($"Dependency cycle found: {string.Join(" > ", DependencyChain.Reverse())}");
            DependencyChainProcesses.Add(Process);

            var Result = OnTraverseDependencyChain(CheckList, DependencyChain, Process, DependencyChainAction);

            DependencyChain.Pop();
            DependencyChainProcesses.Remove(Process);
            return Result;
        }

        public void MarkNodeAsChanged(ICheckList checkList, List<IDependencyNode> roots, DependencyNodeKey.Processes Process) => throw new NotImplementedException();

        public virtual void Check()
        {
            if (CountDownCondition == Conditions.Expression)
                CheckExpression(CountDownToCode, DataType.DateTime, DataType.Date);
            if (HiddenCondition == Conditions.Expression)
                CheckExpression(HiddenExpressionCode, DataType.Boolean);
            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
                CheckExpression(ReadOnlyExpressionCode, DataType.Boolean);
            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                CheckExpression(ValidExpressionCode, DataType.Boolean);
        }

        public virtual DependencyNodeKey DependencyNodeKey => string.IsNullOrEmpty(Code) ? null : new DependencyNodeKey("element", Code);
        public HashSet<DependencyNodeKey> DependentNodes { get; } = new HashSet<DependencyNodeKey>();
        public HashSet<DependencyNodeKey> DependingNodes { get; } = new HashSet<DependencyNodeKey>();

        public static void Copy(Element source, Element destination)
        {
            destination.CheckList = source.CheckList;
            destination.Code = source.Code;
            destination.ReadOnlyExpressionCode = source.ReadOnlyExpressionCode;
            destination.ReadOnlyCondition = source.ReadOnlyCondition;
            destination.HiddenExpressionCode = source.HiddenExpressionCode;
            destination.HiddenCondition = source.HiddenCondition;
            destination.ValidExpressionCode = source.ValidExpressionCode;
            destination.ValidCondition = source.ValidCondition;
            destination.ValidityMessage = source.ValidityMessage;
            destination.SignatureCode = source.SignatureCode;
        }
    }
}
