using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class Sum : IteratorFunction
    {
        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            base.Check(Prototype, IteratorStack);
            if (DataType != DataType.Number)
                throw new ParseException("Operands of function Sum must be numbers", Position);
        }

        protected override TypedValue GetValue(List<TypedValue> Values)
        {
            return new TypedValue(DataType.Number, Values.Any(Value => Value.IsNull) ? (decimal?)null : Values.Sum(TypedValue => TypedValue.NumberValue));
        }

        public override string ToString()
        {
            return $"{IteratorCode}.Sum({Argument?.ToString() ?? "#"})";
        }
    }
}
