using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Power : BinaryArithmeticOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Left.DataType != DataType.Number || Right.DataType != DataType.Number)
                throw new ParseException("Operand of operator ^ must be numbers", Position);

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            if (LeftValue.IsNull || RightValue.IsNull)
                return new TypedValue(DataType.Number, null);

            if (LeftValue.NumberValue == 0 && RightValue.NumberValue == 0)
                throw new Exception("Indeterminate form");

            return new TypedValue(DataType.Number, (decimal)Math.Pow((double)LeftValue.NumberValue, (double)RightValue.NumberValue));
        }

        protected override string OperatorName => "^";
    }
}
