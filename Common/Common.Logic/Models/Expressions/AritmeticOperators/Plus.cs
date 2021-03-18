using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Plus : BinaryArithmeticOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Left.DataType == DataType.Text && Right.DataType == DataType.Text)
                DataType = DataType.Text;
            else if (Left.DataType == DataType.Number)
            {
                if (Right.DataType != DataType.Number)
                    throw new ParseException("Right operand of operator + must be a number when left operand is a number", Position);
                DataType = DataType.Number;
            }
            else if (Left.DataType == DataType.DateTime || Left.DataType == DataType.Time || Left.DataType == DataType.Date)
            {
                if (Right.DataType != DataType.Number)
                    throw new ParseException($"Right operand of operator + must be a number when left operand is a {Left.DataType}", Position);
                DataType = Left.DataType;
            }
            else 
                throw new ParseException($"The operator + cannot be applied to a {Left.DataType} and a {Right.DataType}", Position);
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            DataType = LeftValue.Type == DataType.Date ? DataType.DateTime : LeftValue.Type;

            return
                LeftValue.Type == DataType.Text ? new TypedValue(DataType, LeftValue.TextValue + RightValue.TextValue) :
                LeftValue.Type == DataType.Number ? new TypedValue(DataType, LeftValue.IsNull || RightValue.IsNull ? (decimal?)null : LeftValue.NumberValue + RightValue.NumberValue) :
                LeftValue.Type == DataType.Time ? new TypedValue(DataType, LeftValue.IsNull || RightValue.IsNull ? (TimeSpan?)null : LeftValue.TimeSpanValue.Add(TimeSpan.FromSeconds((double)RightValue.NumberValue))) :
                new TypedValue(DataType, LeftValue.IsNull || RightValue.IsNull ? (DateTimeOffset?)null : LeftValue.DateTimeValue.AddSeconds((double)RightValue.NumberValue));
        }

        protected override string OperatorName => "+";
    }
}
