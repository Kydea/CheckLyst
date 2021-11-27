using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class Error : UnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Argument.DataType != DataType.Text)
                throw new ParseException("The argument of error must be a text", Position);

            DataType = DataType.Error;
        }

        protected override TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue)
            => new TypedValue(DataType.Error, ArgumentValue.TextValue);

        public override string ToString() =>
            $"Error({Argument?.ToString() ?? "#"})";
    }
}
