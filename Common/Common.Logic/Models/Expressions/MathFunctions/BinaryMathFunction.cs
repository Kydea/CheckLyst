using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public abstract class BinaryMathFunction : BinaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Left.DataType != DataType.Number || Right.DataType != DataType.Number)
                throw new ParseException("The arguments of a math functions must be a number", Position);

            DataType = DataType.Number;
        }

        protected abstract TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue);

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue, string TimeZone) =>
            base.Evaluate(LeftValue, RightValue, TimeZone) ?? (LeftValue.IsNull || RightValue.IsNull ? LeftValue : EvaluateResult(LeftValue, RightValue));

        public override string ToString()
        {
            return $"{OperatorName}({Left?.ToString() ?? "#"}, {Right?.ToString() ?? "#"})";
        }
    }
}
