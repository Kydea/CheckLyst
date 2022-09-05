using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using CheckListManager.Instances.Logic.Exceptions;
using Newtonsoft.Json;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using System.Linq;
using System.Dynamic;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public abstract class Element : DependencyNode, ICheckListElement
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

        public virtual Dictionary<string, ICheckListElement> GetBranchElements() => CheckList.Elements;

        public virtual bool Setup()
		{
            Code = string.IsNullOrEmpty(Code) ? null : Code;
            if (Code != null)
				CheckList.Elements[Code] = this;
            if (!(this is ICheckListValidityElement))
                Valid = true;
            return true;
        }

        public override bool Update(DependencyRelation.Properties Property)
        {
            switch (Property)
            {
                case DependencyRelation.Properties.Validity:
                    UpdateValid();
                    break;
                case DependencyRelation.Properties.Hidden:
                    UpdateHidden();
                    break;
                case DependencyRelation.Properties.CountDown:
                    UpdateCountDownTo();
                    break;
                default:
                    throw new ArgumentException();
            }

            return true;
        }

        public virtual dynamic GetViewModel(bool ReportModel)
        {
            dynamic ViewModel = new ExpandoObject();
            ViewModel.Code = Code;
            ViewModel.Valid = Valid;
            ViewModel.ValidityMessage = ValidityMessage;
            ViewModel.ReadOnly = ReadOnly;
            ViewModel.CountDownToValue = CountDownToValue;
            return ViewModel;
        }

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

        public bool ReadOnly { get; protected set; } //set is protected because this property is not persistent
        public Conditions ReadOnlyCondition { get; set; }
        public string ReadOnlyExpressionCode { get; set; }
        protected virtual bool IsReadOnlyByDefault() => false;
        public void UpdateReadOnly(string userCode, bool readOnlyInstance)
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

        public virtual bool PageBreak { get; set; }

        protected virtual void UpdateHidden()
        {
            Hidden =
                HiddenCondition == Conditions.True
                || HiddenCondition == Conditions.Expression && CheckList?.GetExpressionValue(HiddenExpressionCode, this)?.Value as bool? == true;
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
            destination.PageBreak = source.PageBreak;
        }

        #endregion

        #region Dependency

        protected void BindExpression(string ExpressionCode, DependencyRelation.Properties SourceProperty, string PropertyName)
        {
            if (!string.IsNullOrEmpty(ExpressionCode) && CheckList.Expressions.TryGetValue(ExpressionCode, out var Expression))
                AddDependingRelation(SourceProperty, Expression, DependencyRelation.Properties.Value);
        }

        protected void BindElement<T>(string Code, DependencyRelation.Properties SourceProperty, DependencyRelation.Properties DestinationProperty) where T : Element
        {
            var BranchElements = GetBranchElements();
            if (!string.IsNullOrEmpty(Code) && BranchElements.ContainsKey(Code) && BranchElements[Code] is T Element)
                AddDependingRelation(SourceProperty, Element, DestinationProperty);
        }

        public virtual void BuildDependencies()
        {
            if (!string.IsNullOrEmpty(SignatureCode))
                BindElement<Signature>(SignatureCode, DependencyRelation.Properties.ReadOnly, DependencyRelation.Properties.Signature);

            if (CountDownCondition == Conditions.Expression)
                BindExpression(CountDownToCode, DependencyRelation.Properties.CountDown, "countdown");

            if (CountDownCondition == Conditions.Signature)
                BindElement<Signature>(CountDownToCode, DependencyRelation.Properties.CountDown, DependencyRelation.Properties.Signature);

            if (ValidCondition == Conditions.Expression || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                BindExpression(ValidExpressionCode, DependencyRelation.Properties.Validity, "valid");

            if (HiddenCondition == Conditions.Expression)
                BindExpression(HiddenExpressionCode, DependencyRelation.Properties.Hidden, "hidden");

            if (ReadOnlyCondition == Conditions.Expression || ReadOnlyCondition == Conditions.ExpressionOrDefault || ReadOnlyCondition == Conditions.ExpressionAndDefault)
                BindExpression(ReadOnlyExpressionCode, DependencyRelation.Properties.ReadOnly, "read-only");

            if (this is ICheckListValidityElement)
                AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Hidden);
        }

        public virtual void MarkAllAsChanged(HashSet<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            if (this is ICheckListValidityElement)
                MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.Validity);
            MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.Hidden);
            MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.CountDown);
        }
        
        public override string DependencyClassName => "element";

        #endregion
    }
}
