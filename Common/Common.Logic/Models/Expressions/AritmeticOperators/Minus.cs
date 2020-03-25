using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Minus : BinaryArithmeticOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (
                !(Left.DataType == DataType.Number && Right.DataType == DataType.Number)
                && !((Left.DataType == DataType.DateTime || Left.DataType == DataType.Date) && (Right.DataType == DataType.DateTime || Right.DataType == DataType.Date))
                && !(Left.DataType == DataType.Time && Right.DataType == DataType.Time)
            )
                throw new ParseException("Operands of operator - must be both numbers or both date/time", Position);

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            return new TypedValue(DataType.Number,
                LeftValue.IsNull || RightValue.IsNull ? (decimal?)null :
                LeftValue.Type == DataType.Time ? (decimal)(LeftValue.TimeSpanValue - RightValue.TimeSpanValue).TotalSeconds :
                LeftValue.Type == DataType.Number ? LeftValue.NumberValue - RightValue.NumberValue :
                (decimal)(LeftValue.DateTimeValue - RightValue.DateTimeValue).TotalSeconds
            );
        }

        protected override string OperatorName => "-";
    }

}
