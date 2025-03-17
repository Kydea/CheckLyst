using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public class Round : BinaryMathFunction
    {
        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            if (RightValue.NumberValue < 0)
                throw new ParseException("The second argument must be greater or equals to 0", Position);

            return new TypedValue(DataType.Number, Math.Round(LeftValue.NumberValue, (int)RightValue.NumberValue));
        }

        protected override string OperatorName => "Round";
    }
}
