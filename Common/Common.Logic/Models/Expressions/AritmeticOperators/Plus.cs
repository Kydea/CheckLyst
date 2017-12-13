using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Plus : BinaryArithmeticOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            if (Right.DataType != DataType.Number)
                throw new ParseException("Right operand of operator + must be a number", Position);

            if (Left.DataType != DataType.Number && Left.DataType != DataType.DateTime && Left.DataType != DataType.Date && Left.DataType != DataType.Time)
                throw new ParseException("Left operand of operator + must be a number or a date/time", Position);

            DataType = Left.DataType;
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            if (LeftValue.Type == DataType.Number)
                return new TypedValue(DataType.Number, LeftValue.IsNull || RightValue.IsNull ? (decimal?)null : LeftValue.NumberValue + RightValue.NumberValue);
            else
                return new TypedValue(DataType.DateTime, LeftValue.IsNull || RightValue.IsNull ? (DateTime?)null : LeftValue.DateTimeValue.AddSeconds((double)RightValue.NumberValue));
        }

        protected override string OperatorName => "+";
    }
}
