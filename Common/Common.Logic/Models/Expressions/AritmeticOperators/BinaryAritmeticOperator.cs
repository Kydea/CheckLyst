using System;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public abstract class BinaryArithmeticOperator : BinaryOperator
    {
        protected abstract TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue);

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue)
        {
            var Value = base.Evaluate(LeftValue, RightValue);
            if (Value == null)
                try
                {
                    return EvaluateResult(LeftValue, RightValue);
                }
                catch (Exception Exception)
                {
                    return new TypedValue(DataType.Error, Exception.Message);
                }
            else
                return Value;
        }
    }
}
