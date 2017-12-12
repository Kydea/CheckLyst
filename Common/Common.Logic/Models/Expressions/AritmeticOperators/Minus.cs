using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Minus : BinaryArithmeticOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            if (
                !(Left.DataType == DataType.Number && Right.DataType == DataType.Number)
                && !((Left.DataType == DataType.DateTime || Left.DataType == DataType.Date) && (Right.DataType == DataType.DateTime || Right.DataType == DataType.Date))
                && !(Left.DataType == DataType.Time && Right.DataType == DataType.Time)
            )
                throw new Exception("Operands of operator - must be both numbers or both date/time");

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
