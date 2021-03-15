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

            if (Left.DataType == DataType.Number)
            {
                if (Right.DataType != DataType.Number)
                    throw new ParseException("Right operand of operator - must be a number when left operand is a number", Position);
                DataType = DataType.Number;
            }
            else if (Left.DataType == DataType.Time)
            {
                if (Right.DataType == DataType.Number)
                    DataType = Left.DataType;
                else if (Right.DataType == DataType.Time)
                    DataType = DataType.Number;
                else
                    throw new ParseException($"Right operand of operator - must be a number or a time when left operand is a time", Position);
            }
            else if (Left.DataType == DataType.DateTime || Left.DataType == DataType.Date)
            {
                if (Right.DataType == DataType.Number)
                    DataType = Left.DataType;
                else if (Right.DataType == DataType.DateTime || Right.DataType == DataType.Date)
                    DataType = DataType.Number;
                else
                    throw new ParseException($"Right operand of operator - must be a number or a date or a date/time when left operand is a date or a date/time", Position);
            }
            else
                throw new ParseException($"The operator - cannot be applied to a {Left.DataType} and a {Right.DataType}", Position);
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            DataType =
                LeftValue.Type == DataType.Number ? DataType.Number :
                LeftValue.Type == DataType.Time && RightValue.Type == DataType.Text ? DataType.Number :
                LeftValue.Type == DataType.Time ? DataType.Time :
                RightValue.Type == DataType.Number ? DataType.DateTime :
                DataType.Number;

            return 
                LeftValue.IsNull || RightValue.IsNull ? new TypedValue(DataType, null) :
                LeftValue.Type == DataType.Number ? new TypedValue(DataType, LeftValue.NumberValue - RightValue.NumberValue) :
                LeftValue.Type == DataType.Time && RightValue.Type == DataType.Time ? new TypedValue(DataType, (decimal)(LeftValue.TimeSpanValue - RightValue.TimeSpanValue).TotalSeconds) :
                LeftValue.Type == DataType.Time ? new TypedValue(DataType, LeftValue.TimeSpanValue.Add(-TimeSpan.FromSeconds((int)RightValue.NumberValue))) :
                RightValue.Type == DataType.Number ? new TypedValue(DataType, LeftValue.DateTimeValue.AddSeconds(-(int)RightValue.NumberValue)) :
                new TypedValue(DataType, (decimal)(LeftValue.DateTimeValue - RightValue.DateTimeValue).TotalSeconds);
        }

        protected override string OperatorName => "-";
    }

}
