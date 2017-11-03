using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AritmeticOperators
{
    public class Division : BinaryAritmeticOperator
    {

        public override void Check(Prototype prototype)
        {
            base.Check(prototype);

            if (Left.DataType != DataType.Number || Right.DataType != DataType.Number)
                throw new Exception("Operand of operator / must be numbers");

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue leftValue, TypedValue rightValue)
        {
            if (leftValue.IsNull || rightValue.IsNull)
                return new TypedValue(DataType.Number, null);

            if (rightValue.NumberValue == 0)
                throw new Exception("Division by 0");

            return new TypedValue(DataType.Number, leftValue.NumberValue / rightValue.NumberValue);
        }
    }
}
