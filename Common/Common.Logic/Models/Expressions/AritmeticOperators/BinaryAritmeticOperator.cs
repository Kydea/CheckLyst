using System;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public abstract class BinaryArithmeticOperator : BinaryOperator
    {
        protected abstract TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue);

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue, string TimeZone) =>
            base.Evaluate(LeftValue, RightValue, TimeZone) ?? EvaluateResult(LeftValue, RightValue);
    }
}
