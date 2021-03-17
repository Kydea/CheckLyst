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

        public List<Configuration.Error> Errors { get; } = new List<Configuration.Error>();


        public Conditions CountDownCondition { get; set; }
        public string CountDownToCode { get; set; }
        public int CountDownOffset { get; set; }
        public Conditions HiddenCondition { get; set; } = Conditions.False;
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
        public bool PageBreak { get; set; }

        public virtual bool Setup()
        {
            var Result = true;
            Code = string.IsNullOrEmpty(Code) ? null : Code;
            Errors.Clear();
            if (Code != null)
            {
                if (CheckList.Elements.TryGetValue(Code, out var SameCodeElement))
                {
                    var Error = new Configuration.Error("code", $"Duplicated code {Code}");
                    Errors.Add(Error);
                    ((Element)SameCodeElement).Errors.Add(Error);
                    Result = false;
                }
                else
                    CheckList.Elements[Code] = this;
            }
            ClearDependencies();

            return Result;
        }

        protected virtual Dictionary<string, ICheckListElement> GetBranchElements()
        {
            return CheckList.Elements;
        }

        private bool GetExpression(string ExpressionCode, DependencyRelation.Properties SourceProperty, string PropertyName)
        {
            var Result = false;
            if (string.IsNullOrEmpty(ExpressionCode))
                Errors.Add(new Configuration.Error(PropertyName, $"Expression code expected"));
            else if (!CheckList.Expressions.TryGetValue(ExpressionCode, out var Expression))
                Errors.Add(new Configuration.Error(PropertyName, $"Expression {ExpressionCode} not found"));
            else
            {
                AddDependingRelation(SourceProperty, Expression, DependencyRelation.Properties.Value);
                Result = true;
            }

            return Result;
        }

        private bool GetSignature(string SignatureCode, DependencyRelation.Properties SourceProperty, string PropertyName)
        {
            var Result = false;
            var BranchElements = GetBranchElements();
            if (string.IsNullOrEmpty(SignatureCode))
                Errors.Add(new Configuration.Error(PropertyName, $"Signature code expected"));
            else if (!BranchElements.ContainsKey(SignatureCode))
                Errors.Add(new Configuration.Error(PropertyName, "Signature {SignatureCode} not found"));
            else if (!(BranchElements[SignatureCode] is Signature Signature))
                Errors.Add(new Configuration.Error(PropertyName, $"The element {SignatureCode} is not a signature"));
            else
            {
                AddDependingRelation(SourceProperty, Signature, DependencyRelation.Properties.Signature);
                Result = true;
            }
            return Result;
        }

        public virtual bool BuildDependencies()
        {
            var Result = true;
            if (!string.IsNullOrEmpty(SignatureCode))
                Result &= GetSignature(SignatureCode, DependencyRelation.Properties.ReadOnly, "signature");

            if (CountDownCondition == Conditions.Expression)
                Result &= GetExpression(CountDownToCode, DependencyRelation.Properties.CountDown, "countdown");

            if (CountDownCondition == Conditions.Signature)
                Result &= GetSignature(CountDownToCode, DependencyRelation.Properties.CountDown, "signature");

            if (HiddenCondition == Conditions.Expression)
                Result &= GetExpression(HiddenExpressionCode, DependencyRelation.Properties.Hidden, "hidden");

            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
                Result &= GetExpression(ReadOnlyExpressionCode, DependencyRelation.Properties.ReadOnly, "read-only");

            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                Result &= GetExpression(ValidExpressionCode, DependencyRelation.Properties.Validity, "validity");

            if (this is ICheckListValidityElement)
                AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Hidden);

            return Result;
        }

        public override bool Update(DependencyRelation.Properties Property) => true;

        private bool CheckExpression(string Property, string ExpressionCode, params DataType[] DataType)
        {
            if (CheckList.Expressions.TryGetValue(ExpressionCode, out var Expression) && !DataType.Any(dt => dt == (Expression.InnerDataType ?? Expression.DataType)))
            {
                Errors.Add(new Configuration.Error(Property, $"the expression {ExpressionCode} is not of type {string.Join(" or ", DataType.Select(dt => dt.TypeName))}"));
                return false;
            }
            return true;
        }

        public virtual bool Check()
        {
            var Result = true;
            if (CountDownCondition == Conditions.Expression)
                Result &= CheckExpression("countdown", CountDownToCode, DataType.DateTime, DataType.Date);
            if (HiddenCondition == Conditions.Expression)
                Result &= CheckExpression("hidden", HiddenExpressionCode, DataType.Boolean);
            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
                Result &= CheckExpression("read-only", ReadOnlyExpressionCode, DataType.Boolean);
            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                Result &= CheckExpression("validity", ValidExpressionCode, DataType.Boolean);
            return Result;
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
            destination.PageBreak = source.PageBreak;
        }

        public override string DependencyObjectCode => Code;
    }
}
