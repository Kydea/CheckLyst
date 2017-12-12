using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AggregateFunctions
{
    public class Sum : AggregateFunction
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);
            if (DataType != DataType.Number)
                throw new Exception("Operands of functon Sum must be numbers");
        }

        protected override TypedValue GetValue(IEnumerable<TypedValue> Values)
        {
            return new TypedValue(DataType.Number, Values.Any(Value => Value.IsNull) ? (decimal?)null : Values.Sum(TypedValue => TypedValue.NumberValue));
        }

        public override string ToString()
        {
            return $"{IteratorCode}.Sum({Argument?.ToString() ?? "#"})";
        }
    }
}
