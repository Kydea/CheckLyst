using System;
using System.Collections.Generic;
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

		public Expression CountDownTo => CountDownToCode == null ? null : CheckList.Expressions[CountDownToCode];

		public string CountDownToCode { get; set; }
        [JsonIgnore]
        public bool Hidden
		{
			get
			{
				var expressionValue = HiddenExpressionCode != null && CheckList != null && CheckList.Expressions.ContainsKey(HiddenExpressionCode) && CheckList.Expressions[HiddenExpressionCode].Value.Type == DataType.Boolean && CheckList.Expressions[HiddenExpressionCode].Value.BooleanValue;
				return
					HiddenCondition == Conditions.Expression && expressionValue ||
					HiddenCondition == Conditions.True;
			}
		}
		public Conditions HiddenCondition { get; set; }
		public string HiddenExpressionCode { get; set; }
		public Conditions ReadOnlyCondition { get; set; }
		public string ReadOnlyExpressionCode { get; set; }

        [JsonIgnore]
		public ICheckListSignature Signature
			=> SignatureCode == null ? null : (ICheckListSignature)GetBranchElements()[SignatureCode];
		public string SignatureCode { get; set; }

        [JsonIgnore]
        public bool Valid
		{
			get
			{
				if (Hidden) return true;

				var expressionValue = ValidExpressionCode != null && CheckList != null && CheckList.Expressions.ContainsKey(ValidExpressionCode) && CheckList.Expressions[ValidExpressionCode].Value.Type == DataType.Boolean && CheckList.Expressions[ValidExpressionCode].Value.BooleanValue;
				return
					ValidCondition == Conditions.Default && IsValidByDefault() ||
					ValidCondition == Conditions.Expression && expressionValue ||
					ValidCondition == Conditions.ExpressionOrDefault && (expressionValue || IsValidByDefault()) ||
					ValidCondition == Conditions.ExpressionAndDefault && expressionValue && IsValidByDefault() ||
					ValidCondition == Conditions.True;
			}
		}

		public bool ReadOnly { get; set; }
		public Conditions ValidCondition { get; set; }
		public string ValidExpressionCode { get; set; }
		public string ValidityMessage { get; set; }

		public virtual void Setup()
		{
            var ExpressionsInUse = ((Prototype)CheckList).ExpressionsInUse;
            if (CountDownToCode != null) ExpressionsInUse.Add(CountDownToCode);
            if (HiddenExpressionCode != null) ExpressionsInUse.Add(HiddenExpressionCode);
            if (ValidExpressionCode != null) ExpressionsInUse.Add(ValidExpressionCode);
            if (ReadOnlyExpressionCode != null) ExpressionsInUse.Add(ReadOnlyExpressionCode);

            if (Code != null)
                CheckList.Elements[Code] = this;
        }

        public void Update()
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
    }
}
