using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class Format : BinaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Right.DataType != DataType.Text)
                throw new ParseException("The right argument of format must be a text", Position);

            DataType = DataType.Text;
        }

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue)
            => base.Evaluate(LeftValue, RightValue) ?? new TypedValue(DataType.Text, string.Format($"{{0:{LeftValue.TextValue}}}", RightValue.TextValue));

        public override string ToString() =>
            $"{OperatorName}({Left?.ToString() ?? "#"}, {Right?.ToString() ?? "#"})";

        protected override string OperatorName => "Format";
    }
}
