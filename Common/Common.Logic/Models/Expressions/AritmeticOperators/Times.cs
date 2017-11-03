using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AritmeticOperators
{
    public class Times : BinaryAritmeticOperator
    {
        public override void Check(Prototype prototype)
        {
            base.Check(prototype);

            if (Left.DataType != DataType.Number || Right.DataType != DataType.Number)
                throw new Exception("Operand of operator * must be numbers");

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue leftValue, TypedValue rightValue)
        {
            return new TypedValue(DataType.Number, leftValue.IsNull || rightValue.IsNull ? (decimal?)null : leftValue.NumberValue * rightValue.NumberValue);
        }
    }
}
