using System;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.MathFunctions
{
    public abstract class MathFunction : UnaryOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            if (Argument.DataType != DataType.Number)
                throw new ParseException("Left operand of math functions must be numbers", Position);

            DataType = DataType.Number;
        }

        protected override TypedValue Evaluate(TypedValue ArgumentValue)
        {
            return base.Evaluate(ArgumentValue) ?? (ArgumentValue.IsNull ? ArgumentValue : null);
        }

    }
}
