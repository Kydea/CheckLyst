using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public class Abs : UnaryMathFunction
    {
        protected override TypedValue EvaluateResult(TypedValue ArgumentValue)
        {
            return new TypedValue(DataType.Number, Math.Abs(ArgumentValue.NumberValue));
        }

        public override string ToString()
        {
            return $"abs({Argument?.ToString() ?? "#"})";
        }

    }
}
