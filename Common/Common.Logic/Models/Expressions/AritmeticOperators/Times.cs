using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Times : BinaryArithmeticOperator
    {
        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            base.Check(Prototype, IteratorStack);

            if (Left.DataType != DataType.Number || Right.DataType != DataType.Number)
                throw new ParseException("Operand of operator * must be numbers", Position);

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            return new TypedValue(DataType.Number, LeftValue.IsNull || RightValue.IsNull ? (decimal?)null : LeftValue.NumberValue * RightValue.NumberValue);
        }

        protected override string OperatorName => "*";
    }
}
