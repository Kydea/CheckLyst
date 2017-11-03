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

        protected override void Check(ICheckListElement element)
        {
	        if (element is ICheckListSignature)
	        {
		        DataType =
			        _property == SignatureProperties.Signed ? DataType.Boolean :
				        _property == SignatureProperties.SignedBy ? DataType.Text :
					        DataType.DateTime;
	        }
			else
				throw new Exception("The element with code " + Code + " is not an Signature");
        }

        protected override TypedValue GetValue(ICheckListElement element)
        {
            var signature = (ICheckListSignature)element;
            return
                _property == SignatureProperties.Signed ? new TypedValue(DataType.Boolean, signature.Signed) :
                _property == SignatureProperties.SignedBy ? new TypedValue(DataType.Text, signature.Signed ? signature.SignedBy : null) :
                _property == SignatureProperties.SignedOn ? new TypedValue(DataType.DateTime, signature.Signed ? signature.SignedOn : (DateTime?)null) :
                null;
        }
    }
}
