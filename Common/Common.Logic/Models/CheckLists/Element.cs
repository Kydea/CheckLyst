using System;
using System.Collections.Generic;
using System.Dynamic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public abstract class Element : ICheckListElement
	{
        public static void Copy(ICheckListElement source, ICheckListElement destination)
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

        public string ReadOnlyExpressionCode { get; set; }

        protected virtual bool IsReadOnlyByDefault()
        {
            return false;
        }

        public virtual bool IsReadOnly(uint clientId)
        {
			//TODO: check if readonly for the client
            //if (Client.Get(clientId).ReadOnlyInstance) return true;
            var expressionValue = ReadOnlyExpressionCode != null && CheckList != null && CheckList.Expressions.ContainsKey(ReadOnlyExpressionCode) && CheckList.Expressions[ReadOnlyExpressionCode].Value.Type == DataType.Boolean && CheckList.Expressions[ReadOnlyExpressionCode].Value.BooleanValue;
            return
            (
                ReadOnlyCondition == Conditions.Default && IsReadOnlyByDefault() ||
                ReadOnlyCondition == Conditions.Expression && expressionValue ||
                ReadOnlyCondition == Conditions.ExpressionOrDefault && (expressionValue || IsReadOnlyByDefault()) ||
                ReadOnlyCondition == Conditions.ExpressionAndDefault && expressionValue && IsReadOnlyByDefault() ||
                ReadOnlyCondition == Conditions.True
            ) || (Signature?.Signed ?? false);
        }

        public Conditions ReadOnlyCondition { get; set; } = Conditions.Default;

        //[JsonIgnore]
        //public Expression HiddenExpression { get; private set; }

        public string HiddenExpressionCode { get; set; }

        public Conditions HiddenCondition { get; set; } = Conditions.False;

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

        //[JsonIgnore]
        //public Expression ValidExpression { get; private set; }

        public string ValidExpressionCode { get; set; }

        public Conditions ValidCondition { get; set; } = Conditions.Default;

        public string ValidityMessage { get; set; }

        public virtual bool IsValidByDefault()
        {
            return true;
        }

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

        public string Code { get; set; }

        [JsonIgnore]
        public ICheckList CheckList { get; set; }

        [JsonIgnore]
        public Expression CountDownTo => CountDownToCode == null ? null : CheckList.Expressions[CountDownToCode];

	    public string CountDownToCode { get; set; }

        public string SignatureCode { get; set; }

        protected virtual Dictionary<string, ICheckListElement> GetBranchElements()
        {
            return CheckList.Elements;
        }

        [JsonIgnore]
        public Signature Signature => SignatureCode == null ? null : (Signature)GetBranchElements()[SignatureCode];

	    public virtual string GetSignatureInformation()
        {
            return Signature != null && Signature.Signed ? $"by {Signature.SignedBy} on {Signature.SignedOn:dd/MM/yyyy HH:mm}"
	            : null;
        }

        public virtual void Setup()
        {
            if (Code != null)
                CheckList.Elements[Code] = this;
        }

        public abstract void Update();

        public virtual dynamic GetModel(uint clientId)
        {
            dynamic result = new ExpandoObject();
            result.Code = Code;
            result.CountDownTo = CountDownTo != (Expression)null && CountDownTo.Value.Type == DataType.DateTime && !CountDownTo.Value.IsNull ? CountDownTo.Value.DateTimeValue : (DateTime?)null;
            result.ReadOnly = IsReadOnly(clientId);
            result.Hidden = Hidden;
            result.Valid = Valid;
            result.SignatureInformation = GetSignatureInformation();
            result.ValidityMessage = ValidityMessage;
            return result;
        }
    }
}
