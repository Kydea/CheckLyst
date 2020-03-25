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
		public bool ReadOnly { get; set; }
		public Conditions ValidCondition { get; set; }
		public string ValidExpressionCode { get; set; }
		public string ValidityMessage { get; set; }

        [JsonIgnore]
        public bool Hidden => false;
        [JsonIgnore]
        public ICheckListSignature Signature => null;
        [JsonIgnore]
        public bool Valid => true;

        protected virtual DependencyNodeKey DependencyNodeKey => Code == null ? null : new DependencyNodeKey("element", Code);

        public virtual void Setup()
		{
            if (!string.IsNullOrEmpty(Code))
            {
                if (CheckList.Elements.ContainsKey(Code))
                    throw new Exception($"Duplicated code {Code}");
                CheckList.Elements[Code] = this;
            }
        }

        public void Update(string DecimalSeparator, string TimeZone)
		{
			throw new NotImplementedException();
		}

		public void UpdateReadOnly(string userCode, bool canEdit)
		{
			throw new NotImplementedException();
		}

		public virtual bool IsValidByDefault()
		{
			return true;
		}

		protected virtual Dictionary<string, ICheckListElement> GetBranchElements()
		{
			return CheckList.Elements;
		}

        public string GetSignatureInformation()
        {
            throw new NotImplementedException();
        }

        protected Expression GetExpression(string ExpressionCode, DependencyNodeKey DependencyNodeKey, params DataType[] DataType)
        {
            if (ExpressionCode == null) return null;
            if (string.IsNullOrEmpty(ExpressionCode))
                throw new Exception($"The {DependencyNodeKey} reference is missing");
            if (!CheckList.Expressions.ContainsKey(ExpressionCode))
                throw new Exception($"The {DependencyNodeKey} references to the inexistent expression {ExpressionCode}");
            var Expression = CheckList.Expressions[ExpressionCode];
            if (!DataType.Any(dt => dt == (Expression.InnerDataType ?? Expression.DataType)))
                throw new Exception($"The {DependencyNodeKey} references to the expression {ExpressionCode} which value is not {string.Join(" or ", DataType.Select(dt => dt.TypeName))}");
            return Expression;
        }

        protected Signature GetSignature(string SignatureCode, DependencyNodeKey DependencyNodeKey)
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

        public virtual void Check()
        {
            var BranchElements = GetBranchElements();

            var DependencyNodeKey = this.DependencyNodeKey.Clone("signature");
            if (!string.IsNullOrEmpty(SignatureCode))
                GetSignature(SignatureCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("countdown");
            if (CountDownCondition == Conditions.Expression)
                GetExpression(CountDownToCode, DependencyNodeKey, DataType.DateTime, DataType.Date).DependentNodes.Add(DependencyNodeKey);

            if (CountDownCondition == Conditions.Signature)
                GetSignature(CountDownToCode, DependencyNodeKey).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("hidden");
            if (HiddenCondition == Conditions.Expression)
                GetExpression(HiddenExpressionCode, DependencyNodeKey, DataType.Boolean).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("read-only");
            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
                GetExpression(ReadOnlyExpressionCode, DependencyNodeKey, DataType.Boolean).DependentNodes.Add(DependencyNodeKey);

            DependencyNodeKey = this.DependencyNodeKey.Clone("validity");
            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                GetExpression(ValidExpressionCode, DependencyNodeKey, DataType.Boolean).DependentNodes.Add(DependencyNodeKey);
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
    }
}
