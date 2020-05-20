using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class SignatureProperty : ElementProperty
    {
        public const string SIGNED = "property Signed", SIGNED_BY = "property SignedBy", SIGNED_ON = "property SignedOn";

        public SignatureProperty(string Property)
        {
            this.Property = Property;
        }

        protected override DependencyRelation.Properties DependencyProperty => DependencyRelation.Properties.Signature;

        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListSignature)) throw new Exception($"The element with code {Code} is not an Signature");
            DataType =
                Property == SIGNED ? DataType.Boolean :
                Property == SIGNED_BY ? DataType.Text :
                DataType.DateTime;
        }

        protected override TypedValue GetValue(ICheckListElement Element, string TimeZone)
        {
            var Signature = (ICheckListSignature)Element;
            return
                Property == SIGNED ? new TypedValue(DataType.Boolean, Signature.Signed) :
                Property == SIGNED_BY ? new TypedValue(DataType.Text, Signature.Signed ? Signature.SignedBy : null) :
                Property == SIGNED_ON ? new TypedValue(DataType.DateTime, Signature.Signed ? Signature.SignedOn : (DateTimeOffset?)null) :
                null;
        }
    }
}
