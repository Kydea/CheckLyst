using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class FieldProperty : ElementProperty
    {
        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListField)) throw new Exception($"The element with code {Code} is not a Field");
            DataType = ((ICheckListField)Element).ValueType;
        }

        protected override TypedValue GetValue(ICheckListElement Element, string TimeZone)
        {
            var Field = (ICheckListField)Element;
            return new TypedValue(Field.ValueType, Field.Value, Field.LocalTimeZone ? TimeZone : TimeZoneInfo.Utc.Id);
        }

        public override string ToString()
        {
            return $"{Code}.Value";
        }
    }
}
