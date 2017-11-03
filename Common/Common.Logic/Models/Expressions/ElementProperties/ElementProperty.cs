using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class ElementProperty : Symbol
    {
        public enum ElementProperties { Undefined, Valid }

        private readonly ElementProperties _property;

        protected ElementProperty(string code)
        {
            Code = code;
        }

        public ElementProperty(string code, ElementProperties property) : this(code)
        {
            _property = property;
        }

        protected string Code { get; }

        protected virtual void Check(Element element)
        {
			if (_property == ElementProperties.Valid)
				DataType = DataType.Boolean;
		}

        public override void Check(Prototype prototype)
        {
            Check(prototype.Elements[Code]);
        }

        protected virtual TypedValue GetValue(Element element)
        {
            throw new NotImplementedException("Implement this ethod in the subclasses.");
        }

        public override TypedValue GetValue(IElementsBranch elementsBranch)
        {
            var element = elementsBranch.Elements[Code];
            return _property == ElementProperties.Undefined ? GetValue(element) : new TypedValue(DataType.Boolean, element.Valid);
        }
    }
}
