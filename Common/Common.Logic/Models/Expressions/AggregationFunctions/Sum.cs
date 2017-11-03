using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AggregationFunctions
{
    public class Sum : AggregateFunction
    {
        public Sum(string iteratorCode) : base(iteratorCode) { }

        public override void Check(Prototype prototype)
        {
            base.Check(prototype);
            if (DataType != DataType.Number)
                throw new Exception ("Operands of functon Sum must be numbers");
        }

        protected override TypedValue GetValue(IEnumerable<TypedValue> values)
        {
	        var list = values.ToList();
            return new TypedValue(DataType.Number, list.Any(value => value.IsNull)
				? (decimal?)null
				: list.Sum(typedValue => typedValue.NumberValue));
        }
    }
}
