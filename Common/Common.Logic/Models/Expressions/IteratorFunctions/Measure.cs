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
            this.Operation = Operation;
        }

        private string Operation { get; }

        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            base.Check(Prototype, IteratorStack);
            if (DataType != DataType.Number)
                throw new ParseException($"Operands of function {Operation} must be numbers", Position);
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
                Operation == "Sum" ? Values.Sum(TypedValue => TypedValue.NumberValue) :
                Operation == "Avg" ? Values.Average(TypedValue => TypedValue.NumberValue) :
                Operation == "StDev" ? StDev(Values) :
                default(decimal?);

            return new TypedValue(DataType.Number, Result);
        }

        public override string ToString()
        {
            return $"{IteratorCode}.{Operation}({Argument?.ToString() ?? "#"})";
        }
    }
}
