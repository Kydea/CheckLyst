using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public class SqRt : MathFunction
    {
        protected override TypedValue EvaluateResult(TypedValue ArgumentValue)
        {
            if (ArgumentValue.NumberValue < 0)
                throw new ParseException("The arguments of a square root must be greater or equals to 0", Position);

            return new TypedValue(DataType.Number, (decimal)Math.Sqrt((double)ArgumentValue.NumberValue));
        }

        public override string ToString()
        {
            return $"sqrt({Argument?.ToString() ?? "#"})";
        }

    }
}
