using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class ElementProperty : Symbol
    {
        public enum ElementProperties { Valid }

        private ElementProperties Property;

        protected ElementProperty()
        {
        }

        public ElementProperty(ElementProperties Property)
        {
            this.Property = Property;
        }

        public String Code { get; set; }

        protected virtual void Check(ICheckListElement Element)
        {
            if (Property == ElementProperties.Valid)
                DataType = DataType.Boolean;
        }

        public override void Check(ICheckList Prototype)
        {
            if (!Prototype.Elements.ContainsKey(Code))
                throw new ParseException($"Element with code {Code} not found", Position);

            Check(Prototype.Elements[Code]);
        }

        protected virtual TypedValue GetValue(ICheckListElement Element)
        {
            return new TypedValue(DataType.Boolean, Element.Valid);
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch)
        {
            var Element = ElementsBranch.Elements[Code];
            return GetValue(Element);
        }

        public override string ToString()
        {
            return $"{Code}.Valid";
        }
    }
}
