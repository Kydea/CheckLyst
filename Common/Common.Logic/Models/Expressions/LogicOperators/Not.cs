using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public class Not : UnaryOperator
    {
        public override void Check(ICheckList Prototype, Expression Expression, List<string> IteratorStack)
        {
            base.Check(Prototype, Expression, IteratorStack);

            if (Argument.DataType != DataType.Boolean)
                throw new ParseException("Left operand of logic operators must be a boolean", Position);

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
