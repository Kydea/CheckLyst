using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class FieldProperty : ElementProperty
    {
        public FieldProperty(string code) : base(code)
        {
        }

        protected override void Check(Element element)
        {
            if (!(element is Field)) throw new Exception("The element with code " + Code + " is not an Field");
            DataType = ((Field)element).ValueType;
        }

        protected override TypedValue GetValue(Element element)
        {
            var field = (Field)element;
            return new TypedValue(field.ValueType, field.GetValue());
        }
    }
}
