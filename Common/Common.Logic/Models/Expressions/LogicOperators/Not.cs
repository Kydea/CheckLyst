using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public class Not : UnaryOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            if (Argument.DataType != DataType.Boolean)
                throw new Exception("Left operand of logic operators must be a boolean");

            DataType = DataType.Boolean;

        }

        protected override TypedValue Evaluate(TypedValue ArgumentValue)
        {
            return base.Evaluate(ArgumentValue) ?? new TypedValue(DataType.Boolean, ArgumentValue.IsNull || !ArgumentValue.BooleanValue);
        }

        public override string ToString()
        {
            return $"not({Argument?.ToString() ?? "#"})";
        }

    }
}
