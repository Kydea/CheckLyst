using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class Replace : EnnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            if (Arguments.Count != 3 || Arguments.Any(a => a == null)) throw new NullReferenceException("Software design error");

            base.Check(CheckList, Expression, IteratorStack);
            if (Arguments[0].DataType != DataType.Text) throw new ParseException($"The first argument of {OperatorName} must be a text", Position);
            if (Arguments[1].DataType != DataType.Text) throw new ParseException($"The second argument of {OperatorName} must be a text", Position);
            if (Arguments[2].DataType != DataType.Text) throw new ParseException($"The third argument of {OperatorName} must be a text", Position);
        }

        protected override DataType GetDataType() => DataType.Text;

        protected override string OperatorName => "StringReplace";

        public override TypedValue GetValue(List<TypedValue> ArgumentValues, Dictionary<string, int> IterationIndexes, string TimeZone) =>
            base.GetValue(ArgumentValues, IterationIndexes, TimeZone) ??
            new TypedValue(
                DataType.Text,
                ArgumentValues[0].TextValue.Replace(ArgumentValues[1].TextValue, ArgumentValues[2].TextValue)
            );
    }
}
