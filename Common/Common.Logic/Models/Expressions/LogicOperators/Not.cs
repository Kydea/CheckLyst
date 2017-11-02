using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public class Not : UnaryOperator
    {
        public override void Check(Prototype prototype)
        {
            base.Check(prototype);

            if (Argument.DataType != DataType.Boolean)
                throw new Exception("Left operand of logic operators must be a boolean");

            DataType = DataType.Boolean;

        }

        protected override TypedValue Evaluate(TypedValue argumentValue)
        {
            return base.Evaluate(argumentValue) ?? new TypedValue(DataType.Boolean, argumentValue.IsNull || !argumentValue.BooleanValue);
        }
    }
}
