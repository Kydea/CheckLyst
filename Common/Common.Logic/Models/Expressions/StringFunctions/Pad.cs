using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class Pad : EnnaryOperator
    {
        private enum Directions { Left, Right }
        private Directions Direction { get; }
        private string FunctionName { get; }

        public Pad(string FunctionName)
        {
            Direction = FunctionName == "stringleftpad" ? Directions.Left : FunctionName == "stringrightpad" ? Directions.Right : throw new ArgumentException();
            this.FunctionName = Direction == Directions.Left ? "StringLeftPad" : "StringRightPad";
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            if (Arguments.Count != 3 || Arguments.Any(a => a == null)) throw new NullReferenceException("Software design error");

            base.Check(CheckList, Expression, IteratorStack);
            if (Arguments[0].DataType != DataType.Text) throw new ParseException($"The first argument of {FunctionName} must be a text", Position);
            if (Arguments[1].DataType != DataType.Number) throw new ParseException($"The second argument of {FunctionName} must be a number", Position);
            if (Arguments[2].DataType != DataType.Text) throw new ParseException($"The third argument of {FunctionName} must be a text", Position);
        }

        protected override DataType GetDataType() => DataType.Text;

        protected override string OperatorName => FunctionName;

        public override TypedValue GetValue(List<TypedValue> ArgumentValues, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            var Result = ArgumentValues.FirstOrDefault(v => v.Type == DataType.Error) ??
            (
                ArgumentValues[0].IsNull ? ArgumentValues[0] :
                ArgumentValues[1].IsNull ? ArgumentValues[1] :
                null
            );
            if (Result == null)
            {
                var Text = ArgumentValues[0].TextValue;
                var Length = (int)ArgumentValues[1].NumberValue;
                var Pattern = ArgumentValues[2].IsNull ? " " : ArgumentValues[2].TextValue;
                while (Text.Length < Length && Pattern.Length > 0)
                    Text = (Direction == Directions.Left ? Pattern : string.Empty) + Text + (Direction == Directions.Right ? Pattern : string.Empty);
                Result = new TypedValue(DataType.Text, Text);
            }
            return Result;
        }
    }
}
