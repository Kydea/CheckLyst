using System.Collections.Generic;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using System;
using CheckListManager.Instances.Logic.Exceptions;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public abstract class Element : ICheckListElement
    {
        public ICheckList CheckList { get; set; }
        public string Code { get; set; }

        public Expression CountDownTo => string.IsNullOrEmpty(CountDownToCode) ? null : CheckList.Expressions[CountDownToCode];

        public DateTimeOffset? CountDownToValue => CountDownTo != null && CountDownTo.Value != null && CountDownTo.Value.Type == DataType.DateTime && !CountDownTo.Value.IsNull ? CountDownTo.Value.DateTimeValue : (DateTimeOffset?)null;

        public string CountDownToCode { get; set; }
        public virtual bool Hidden
        {
            get
            {
                var expressionValue = 
                    HiddenExpressionCode != null 
                    && CheckList != null 
                    && CheckList.Expressions.ContainsKey(HiddenExpressionCode) 
                    && CheckList.Expressions[HiddenExpressionCode].Value?.Type == DataType.Boolean 
                    && CheckList.Expressions[HiddenExpressionCode].Value.BooleanValue;
                return
                    HiddenCondition == Conditions.Expression && expressionValue ||
                    HiddenCondition == Conditions.True;
            }
        }
        public Conditions HiddenCondition { get; set; }
        public string HiddenExpressionCode { get; set; }
        public Conditions ReadOnlyCondition { get; set; }
        public string ReadOnlyExpressionCode { get; set; }

        public ICheckListSignature Signature
        {
            get
            {
                var BranchElements = GetBranchElements();
                if (SignatureCode == null || BranchElements == null) return null;
                if (!BranchElements.ContainsKey(SignatureCode))
                    throw new InstanceException((string.IsNullOrEmpty(Code) ? "An element" : $"The element {Code}") + $" references to the inexistent signature {SignatureCode}");
                if (!(BranchElements[SignatureCode] is ICheckListSignature Signature))
                    throw new InstanceException((string.IsNullOrEmpty(Code) ? "An element" : $"The element {Code}") + $" references to a signature with code {SignatureCode} that is not a signature");
                return Signature;
            }
        }
		public string SignatureCode { get; set; }
        public virtual string GetSignatureInformation()
            => Signature != null && Signature.Signed ? string.Format("by {0} on {1:yyyy-MM-dd HH:mm}", Signature.SignedBy, Signature.SignedOn) : null;

        public virtual bool Valid
		{
			get
			{
				if (Hidden) return true;

				var expressionValue = 
                    ValidExpressionCode != null 
                    && CheckList != null 
                    && CheckList.Expressions.ContainsKey(ValidExpressionCode) 
                    && CheckList.Expressions[ValidExpressionCode].Value?.Type == DataType.Boolean 
                    && CheckList.Expressions[ValidExpressionCode].Value.BooleanValue;
				return
					ValidCondition == Conditions.Default && IsValidByDefault() ||
					ValidCondition == Conditions.Expression && expressionValue ||
					ValidCondition == Conditions.ExpressionOrDefault && (expressionValue || IsValidByDefault()) ||
					ValidCondition == Conditions.ExpressionAndDefault && expressionValue && IsValidByDefault() ||
					ValidCondition == Conditions.True;
			}
		}

		public Conditions ValidCondition { get; set; }
		public string ValidExpressionCode { get; set; }
		public virtual string ValidityMessage { get; set; }

		public virtual bool IsValidByDefault()
		{
			return true;
		}

		protected virtual Dictionary<string, ICheckListElement> GetBranchElements()
		{
			return CheckList.Elements;
		}

		public virtual void Setup()
		{
            if (!string.IsNullOrEmpty(Code))
				CheckList.Elements[Code] = this;
		}

		public abstract void Update(string DecimalSeparator);

		public bool ReadOnly { get; protected set; }

		protected virtual bool IsReadOnlyByDefault() => false;

		public virtual void UpdateReadOnly(string userCode, bool readOnlyInstance)
		{
			if (readOnlyInstance)
			{
				ReadOnly = true;
				return;
			}

			var expressionValue = 
                ReadOnlyExpressionCode != null && CheckList != null &&
			    CheckList.Expressions.ContainsKey(ReadOnlyExpressionCode) &&
			    CheckList.Expressions[ReadOnlyExpressionCode].Value?.Type == DataType.Boolean &&
			    CheckList.Expressions[ReadOnlyExpressionCode].Value.BooleanValue;

			ReadOnly = (Signature?.Signed ?? false) ||
				ReadOnlyCondition == Conditions.Default && IsReadOnlyByDefault() ||
				ReadOnlyCondition == Conditions.Expression && expressionValue ||
				ReadOnlyCondition == Conditions.ExpressionOrDefault && (expressionValue || IsReadOnlyByDefault()) ||
				ReadOnlyCondition == Conditions.ExpressionAndDefault && expressionValue && IsReadOnlyByDefault() ||
				ReadOnlyCondition == Conditions.True;
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
