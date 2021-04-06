using System;
using System.Collections.Generic;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class Length : UnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Argument.DataType != DataType.Text)
                throw new ParseException("The argument of StringLength must be a text", Position);

            DataType = DataType.Number;
        }

        protected override TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue) =>
            base.Evaluate(ElementsBranch, ArgumentValue) ?? new TypedValue(DataType.Number, ArgumentValue.IsNull ? default(decimal?) : ArgumentValue.TextValue.Length);

        public override string ToString() =>
            $"StringLength({Argument?.ToString() ?? "#"})";
    }
}