using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ArithmeticOperators
{
    public class Times : BinaryArithmeticOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            if (Left.DataType != DataType.Number || Right.DataType != DataType.Number)
                throw new Exception("Operand of operator * must be numbers");

            DataType = DataType.Number;
        }

        protected override TypedValue EvaluateResult(TypedValue LeftValue, TypedValue RightValue)
        {
            return new TypedValue(DataType.Number, LeftValue.IsNull || RightValue.IsNull ? (decimal?)null : LeftValue.NumberValue * RightValue.NumberValue);
        }

        protected override string OperatorName => "*";
    }
}
