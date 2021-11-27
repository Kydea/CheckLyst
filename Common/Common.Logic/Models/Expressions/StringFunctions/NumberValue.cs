using System;
using System.Collections.Generic;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class NumberValue : UnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Argument.DataType != DataType.Text)
                throw new ParseException("The argument of StringToNumber must be a text", Position);

            DataType = DataType.Number;
        }

        protected override TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue) =>
            base.Evaluate(ElementsBranch, ArgumentValue) ?? new TypedValue(DataType.Number, ArgumentValue.TextValue, null);

        public override string ToString() =>
            $"StringToNumber({Argument?.ToString() ?? "#"})";
    }
}