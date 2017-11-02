using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public abstract class BiaryLogicOperator : BinaryOperator
    {
        public override void Check(Prototype prototype)
        {
            base.Check(prototype);

            if (Left.DataType != DataType.Boolean)
                throw new Exception("Left operand of logic operators must be a boolean");

            if (Right.DataType != DataType.Boolean)
                throw new Exception("Right operand of logic operators must be a boolean");

            DataType = DataType.Boolean;
        }
    }
}
