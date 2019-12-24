using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public abstract class UnaryMathFunction : UnaryOperator
    {
        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            base.Check(Prototype, IteratorStack);

            if (Argument.DataType != DataType.Number)
                throw new ParseException("The argument of a math functions must be a number", Position);

            DataType = DataType.Number;
        }

        protected abstract TypedValue EvaluateResult(TypedValue ArgumentValue);

        protected override TypedValue Evaluate(TypedValue ArgumentValue)
        {
            var Value = base.Evaluate(ArgumentValue);
            if (Value == null)
                try
                {
                    return ArgumentValue.IsNull ? ArgumentValue : EvaluateResult(ArgumentValue);
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
