using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public class LN : UnaryMathFunction
    {
        protected override TypedValue EvaluateResult(TypedValue ArgumentValue)
        {
            if (ArgumentValue.NumberValue <= 0)
                throw new ParseException("The arguments of the logarithm must be strictly greater then 0", Position);

            return new TypedValue(DataType.Number, (decimal)Math.Log((double)ArgumentValue.NumberValue));
        }

        public override string ToString()
        {
            return $"ln({Argument?.ToString() ?? "#"})";
        }

    }
}
