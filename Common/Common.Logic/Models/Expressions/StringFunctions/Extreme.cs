using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class Extreme : BinaryOperator
    {
        private enum Directions { Left, Right }
        private Directions Direction { get; }
        private string FunctionName { get; }

        public Extreme(string FunctionName)
        {
            Direction = FunctionName == "stringleft" ? Directions.Left : FunctionName == "stringright" ? Directions.Right : throw new ArgumentException();
            this.FunctionName = Direction == Directions.Left ? "StringLeft" : "StringRight";
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Left.DataType != DataType.Text) throw new ParseException($"The left argument of {OperatorName} must be a text", Position);
            if (Right.DataType != DataType.Number) throw new ParseException($"The right argument of {OperatorName} must be a number", Position);

            DataType = DataType.Text;
        }

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue, string TimeZone)
        {
            var Result = base.Evaluate(LeftValue, RightValue, TimeZone);
            if (Result == null)
            {
                var Text = LeftValue.TextValue;
                var Length = (int)RightValue.NumberValue;
                if (Length > Text.Length) return new TypedValue(DataType.Text, Text);
                Result = new TypedValue(DataType.Text, Direction == Directions.Left ? Text.Substring(0, Length) : Text.Substring(Text.Length - Length));
            }
            return Result;
        }

        public override string ToString() =>
            $"{OperatorName}({Left?.ToString() ?? "#"}, {Right?.ToString() ?? "#"})";

        protected override string OperatorName => FunctionName;
    }
}
