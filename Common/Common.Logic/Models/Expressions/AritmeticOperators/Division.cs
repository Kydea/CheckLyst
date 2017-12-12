using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Division : BinaryArithmeticOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            if (Left.DataType != DataType.Number || Right.DataType != DataType.Number)
                throw new Exception("Operand of operator / must be numbers");

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            if (LeftValue.IsNull || RightValue.IsNull)
                return new TypedValue(DataType.Number, null);

            if (RightValue.NumberValue == 0)
                throw new Exception("Division by 0");

            return new TypedValue(DataType.Number, LeftValue.NumberValue / RightValue.NumberValue);
        }

        protected override string OperatorName => "/";
    }
}
