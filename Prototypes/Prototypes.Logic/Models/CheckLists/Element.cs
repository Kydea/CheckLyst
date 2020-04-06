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
    public abstract class Element : DependencyNode, ICheckListElement
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

        private Expression GetExpression(string ExpressionCode, DependencyRelation.Properties SourceProperty, string PropertyName)
        {
            if (string.IsNullOrEmpty(ExpressionCode))
                throw new Exception($"The property {PropertyName} reference is missing for {this}");
            if (!CheckList.Expressions.TryGetValue(ExpressionCode, out var Expression))
                throw new Exception($"The property {PropertyName} references to the inexistent expression {ExpressionCode} for {this}");
            AddDependingRelation(SourceProperty, Expression, DependencyRelation.Properties.Value);
            return Expression;
        }

        private Signature GetSignature(string SignatureCode, DependencyRelation.Properties SourceProperty, string PropertyName)
        {
            var BranchElements = GetBranchElements();
            if (string.IsNullOrEmpty(SignatureCode))
                throw new Exception($"The property {PropertyName} reference is missing for {this}");
            if (!BranchElements.ContainsKey(SignatureCode))
                throw new Exception($"The property {PropertyName} references to the inexistent signature {SignatureCode} for {this}");
            if (!(BranchElements[SignatureCode] is Signature Signature))
                throw new Exception($"The property {PropertyName} references to the element {SignatureCode} which is not a signature for {this}");
            AddDependingRelation(SourceProperty, Signature, DependencyRelation.Properties.Signature);
            return Signature;
        }

        public virtual void BuildDependencies()
        {
            if (!string.IsNullOrEmpty(SignatureCode))
                GetSignature(SignatureCode, DependencyRelation.Properties.ReadOnly, "signature");

            if (CountDownCondition == Conditions.Expression)
                GetExpression(CountDownToCode, DependencyRelation.Properties.CountDown, "countdown");

            if (CountDownCondition == Conditions.Signature)
                GetSignature(CountDownToCode, DependencyRelation.Properties.CountDown, "signature");

            if (HiddenCondition == Conditions.Expression)
                GetExpression(HiddenExpressionCode, DependencyRelation.Properties.Hidden, "hidden");

            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
                GetExpression(ReadOnlyExpressionCode, DependencyRelation.Properties.ReadOnly, "read-only");

            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                GetExpression(ValidExpressionCode, DependencyRelation.Properties.Validity, "valid");

            AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Hidden);
        }

        public override bool Update(DependencyRelation.Properties Property) => true;

        private void CheckExpression(string ExpressionCode, params DataType[] DataType)
        {
            var Expression = CheckList.Expressions[ExpressionCode];
            if (!DataType.Any(dt => dt == (Expression.InnerDataType ?? Expression.DataType)))
                throw new Exception($"The {this} references to the expression {ExpressionCode} which value is not {string.Join(" or ", DataType.Select(dt => dt.TypeName))}");
        }

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

        public override string ToString() =>
            this is Page Page ? $"page {Page.Index}" :
            this is Group Group ? $"group {Code ?? Group.Id.ToString()}" :
            this is Item Item ? $"item {Code ?? Item.Id.ToString()}" :
            throw new ArgumentException();

        public override string DependencyObjectCode => Code;
    }
}
