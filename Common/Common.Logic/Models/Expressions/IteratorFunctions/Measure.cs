using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class Measure : UnaryIteratorFunction
    {
        private readonly string[] Operations = { "Sum", "Avg", "StDev" };

        public Measure(string Operation)
        {
            if (!Operation.In(Operation)) throw new ArgumentException($"Invalid operation {Operation}");
            Property = Operation;
        }

        protected override string Property { get; }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);
            if (DataType != DataType.Number)
                throw new ParseException($"Operands of function {Property} must be numbers", Position);
            DataType = DataType.Number;
        }

        private decimal StDev (List<TypedValue> Values)
        {
            var Avg = Values.Average(TypedValue => TypedValue.NumberValue);
            return (decimal)Math.Sqrt(Values.Sum(TypedValue => Math.Pow((double)(TypedValue.NumberValue - Avg), 2)) / Values.Count);
        }

        protected override TypedValue GetValue(List<TypedValue> Values)
        {
            var Result =
                Values.Any(Value => Value.IsNull) ? default(decimal?) :
                Property == "Sum" ? Values.Sum(TypedValue => TypedValue.NumberValue) :
                Values.Count == 0 ? default(decimal?) :
                Property == "Avg" ? Values.Average(TypedValue => TypedValue.NumberValue) :
                Property == "StDev" ? StDev(Values) :
                default(decimal?);

            return new TypedValue(DataType.Number, Result);
        }
    }
}
