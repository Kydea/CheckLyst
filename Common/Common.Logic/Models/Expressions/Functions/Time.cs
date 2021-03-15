using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class Time : BinaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);
            if (Left.DataType != DataType.Number) throw new ParseException($"The first argument of {OperatorName} must be a number", Position);
            if (Right.DataType != DataType.Number) throw new ParseException($"The second argument of {OperatorName} must be a number", Position);
            DataType = DataType.Time;
        }

        protected override string OperatorName => nameof(Time);

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue) =>
            base.Evaluate(LeftValue, RightValue) ??
            new TypedValue(DataType.Time, new TimeSpan((int)LeftValue.NumberValue, (int)RightValue.NumberValue, 0));
    }
}
