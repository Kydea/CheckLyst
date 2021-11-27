using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public class Log : UnaryMathFunction
    {
        protected override TypedValue EvaluateResult(TypedValue ArgumentValue)
        {
            if (ArgumentValue.NumberValue <= 0)
                throw new ParseException("The arguments of the logarithm must be strictly greater than 0", Position);

            return new TypedValue(DataType.Number, (decimal)Math.Log10((double)ArgumentValue.NumberValue));
        }

        public override string ToString()
        {
            return $"log({Argument?.ToString() ?? "#"})";
        }

    }
}
