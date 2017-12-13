using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class FieldProperty : ElementProperty
    {
        protected override void Check(ICheckListElement Element)
        {
            if (!(Element is ICheckListField)) throw new Exception("The element with code " + Code + " is not an Field");
            DataType = ((ICheckListField)Element).ValueType;
        }

        protected override TypedValue GetValue(ICheckListElement Element)
        {
            var Field = (ICheckListField)Element;
            return new TypedValue(Field.ValueType, "");
        }

        public override string ToString()
        {
            return $"{Code}.Value";
        }
    }
}
