using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public class Sin : UnaryMathFunction
    {
        protected override TypedValue EvaluateResult(TypedValue ArgumentValue)
        {
            return new TypedValue(DataType.Number, (decimal)Math.Sin((double)ArgumentValue.NumberValue));
        }

        public override string ToString()
        {
            return $"sin({Argument?.ToString() ?? "#"})";
        }

    }
}
