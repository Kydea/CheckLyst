using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class SignatureProperty : ElementProperty
    {
        public enum SignatureProperties { Signed, SignedBy, SignedOn }

        private readonly SignatureProperties _property;

        public SignatureProperty(string code, SignatureProperties property) : base(code)
        {
            _property = property;
        }

        protected override void Check(Element element)
        {
            if (!(element is CheckLists.Signature)) throw new Exception("The element with code " + Code + " is not an Signature");
            var signature = (CheckLists.Signature)element;
            DataType =
                _property == SignatureProperties.Signed ? DataType.Boolean :
                _property == SignatureProperties.SignedBy ? DataType.Text :
                DataType.DateTime;
        }

        protected override TypedValue GetValue(Element element)
        {
            var signature = (CheckLists.Signature)element;
            return
                _property == SignatureProperties.Signed ? new TypedValue(DataType.Boolean, signature.Signed) :
                _property == SignatureProperties.SignedBy ? new TypedValue(DataType.Text, signature.Signed ? signature.SignedBy : null) :
                _property == SignatureProperties.SignedOn ? new TypedValue(DataType.DateTime, signature.Signed ? signature.SignedOn : (DateTime?)null) :
                null;
        }
    }
}
