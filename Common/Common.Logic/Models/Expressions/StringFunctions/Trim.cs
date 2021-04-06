using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class Trim : BinaryOperator
    {
        private enum Directions { Left, Right, Both }
        private Directions Direction { get; }
        private string FunctionName { get; }

        public Trim(string FunctionName)
        {
            Direction = FunctionName == "stringlefttrim" ? Directions.Left : FunctionName == "stringrighttrim" ? Directions.Right : FunctionName == "stringtrim" ? Directions.Both : throw new ArgumentException();
            this.FunctionName = Direction == Directions.Left ? "StringLeftTrim" : Direction == Directions.Right ? "StringRightTrim" : "StringTrim";
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Left.DataType != DataType.Text) throw new ParseException("The left argument of StringRight must be a text", Position);
            if (Right.DataType != DataType.Text) throw new ParseException("The right argument of StringRight must be a text", Position);

            DataType = DataType.Text;
        }

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue, string TimeZone)
        {
            var Result = base.Evaluate(LeftValue, RightValue, TimeZone);
            if (Result == null)
            {
                var Text = LeftValue.TextValue;
                var Pattern = RightValue.IsNull ? " " : RightValue.TextValue;
                if (Direction != Directions.Right)
                    while (Text.StartsWith(Pattern))
                        Text = Text.Substring(Pattern.Length);
                if (Direction != Directions.Left)
                    while (Text.EndsWith(Pattern))
                        Text = Text.Substring(Text.Length - Pattern.Length);
                Result = new TypedValue(DataType.Text, Text);
            }
            return Result;
        }

        public override string ToString() =>
            $"{FunctionName}({Left?.ToString() ?? "#"}, {Right?.ToString() ?? "#"})";

        protected override string OperatorName => FunctionName;
    }
}
