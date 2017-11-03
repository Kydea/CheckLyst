using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AritmeticOperators
{
    public class Plus : BinaryAritmeticOperator
    {
        public override void Check(ICheckList prototype)
        {
            base.Check(prototype);

            if (Right.DataType != DataType.Number)
                throw new Exception("Right operand of operator + must be a number");

            if (Left.DataType != DataType.Number && Left.DataType != DataType.DateTime && Left.DataType != DataType.Date && Left.DataType != DataType.Time)
                throw new Exception("Left operand of operator + must be a number or a date/time");

            DataType = Left.DataType;
        }

        protected override TypedValue EvaluateResult(TypedValue leftValue, TypedValue rightValue)
        {
	        return leftValue.Type == DataType.Number
		        ? new TypedValue(DataType.Number,
			        leftValue.IsNull || rightValue.IsNull ? (decimal?)null : leftValue.NumberValue + rightValue.NumberValue)
		        : new TypedValue(DataType.DateTime,
			        leftValue.IsNull || rightValue.IsNull
				        ? (DateTime?)null
				        : leftValue.DateTimeValue.AddSeconds((double)rightValue.NumberValue));
        }
    }
}
