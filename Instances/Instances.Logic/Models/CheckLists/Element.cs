using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using CheckListManager.Instances.Logic.Exceptions;
using Newtonsoft.Json;

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

        public ICheckList CheckList { get; set; }
        public string Code { get; set; }

        public Conditions CountDownCondition { get; set; }
        public DateTimeOffset? CountDownToValue {
            get
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
                return Value;
            }
        }
        public string CountDownToCode { get; set; }
        public int CountDownOffset { get; set; }

        public virtual bool Hidden
        {
            get
            {
                return
                    HiddenCondition == Conditions.Expression && CheckList?.GetExpressionValue(HiddenExpressionCode, this)?.Value as bool? == true
                    || HiddenCondition == Conditions.True;
            }
        }
        public Conditions HiddenCondition { get; set; }
        public string HiddenExpressionCode { get; set; }

        public Conditions ReadOnlyCondition { get; set; }
        public string ReadOnlyExpressionCode { get; set; }

        public ICheckListSignature Signature => GetSignature(SignatureCode);

        public string SignatureCode { get; set; }
        public virtual string GetSignatureInformation()
            => Signature != null && Signature.Signed ? string.Format("by {0} on {1:yyyy-MM-dd HH:mm}", Signature.SignedBy, Signature.SignedOn) : null;

        public virtual bool Valid
		{
			get
			{
				if (Hidden) return true;

                var expressionValue = CheckList?.GetExpressionValue(ValidExpressionCode, this)?.Value as bool? == true;
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

		public abstract void Update(string DecimalSeparator, string TimeZone);

		public bool ReadOnly { get; protected set; }

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
	}
}
