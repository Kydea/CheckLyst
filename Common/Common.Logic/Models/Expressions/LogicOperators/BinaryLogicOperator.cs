using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public abstract class BiaryLogicOperator : BinaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Left.DataType != DataType.Boolean)
                throw new ParseException("The left operand of logic operators must be a boolean", Position);

            if (Right.DataType != DataType.Boolean)
                throw new ParseException("The right operand of logic operators must be a boolean", Position);

            DataType = DataType.Boolean;
        }
    }
}
