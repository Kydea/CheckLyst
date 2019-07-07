using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class SignatureProperty : ElementProperty
    {
        public enum SignatureProperties { Signed, SignedBy, SignedOn }

        private SignatureProperties Property;

        public SignatureProperty(SignatureProperties Property)
        {
            this.Property = Property;
        }

        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListSignature)) throw new Exception($"The element with code {Code} is not an Signature");
            var Signature = (ICheckListSignature)Element;
            DataType =
                Property == SignatureProperties.Signed ? DataType.Boolean :
                Property == SignatureProperties.SignedBy ? DataType.Text :
                DataType.DateTime;
        }

        protected override TypedValue GetValue(ICheckListElement Element)
        {
            var Signature = (ICheckListSignature)Element;
            return
                Property == SignatureProperties.Signed ? new TypedValue(DataType.Boolean, Signature.Signed) :
                Property == SignatureProperties.SignedBy ? new TypedValue(DataType.Text, Signature.Signed ? Signature.SignedBy : null) :
                Property == SignatureProperties.SignedOn ? new TypedValue(DataType.DateTime, Signature.Signed ? Signature.SignedOn : (DateTimeOffset?)null) :
                null;
        }

        public override string ToString()
        {
            return $"{Code}.{(Property == SignatureProperties.Signed ? "Signed" : Property == SignatureProperties.SignedBy ? "SignedBy" : Property == SignatureProperties.SignedOn ? "SignedOn" : "#")}";
        }
    }
}
