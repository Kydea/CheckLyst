using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class Null : UnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);
            DataType = Argument.DataType;
        }

        protected override TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue)
            => new TypedValue(ArgumentValue.Type, null);

        public override string ToString() =>
            $"Null({Argument?.ToString() ?? "#"})";
    }
}
