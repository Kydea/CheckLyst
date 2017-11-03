using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AritmeticOperators
{
    public class Minus : BinaryAritmeticOperator
    {
        public override void Check(Prototype prototype)
        {
            base.Check(prototype);

            if (
				!(Left.DataType == DataType.Number && Right.DataType == DataType.Number)
				&& !((Left.DataType == DataType.DateTime || Left.DataType == DataType.Date) && (Right.DataType == DataType.DateTime || Right.DataType == DataType.Date))
				&& !(Left.DataType == DataType.Time && Right.DataType == DataType.Time)
			)
				throw new Exception("Operands of operator - must be both numbers or both date/time");

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue leftValue, TypedValue rightValue)
        {
            return new TypedValue(DataType.Number,
                leftValue.IsNull || rightValue.IsNull ? (decimal?)null :
				leftValue.Type == DataType.Time ? (decimal)(leftValue.TimeSpanValue - rightValue.TimeSpanValue).TotalSeconds :
                leftValue.Type == DataType.Number ? leftValue.NumberValue - rightValue.NumberValue :
                (decimal)(leftValue.DateTimeValue - rightValue.DateTimeValue).TotalSeconds
            );
        }
    }
}
