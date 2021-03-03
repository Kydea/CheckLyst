using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class Date : EnnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);
            if (Arguments.Count != 3 || Arguments.Any(a => a == null)) throw new NullReferenceException("Software design error");
            if (Arguments[0].DataType != DataType.Number) throw new ParseException($"The first argument of {OperatorName} must be a number", Position);
            if (Arguments[1].DataType != DataType.Number) throw new ParseException($"The second argument of {OperatorName} must be a number", Position);
            if (Arguments[2].DataType != DataType.Number) throw new ParseException($"The third argument of {OperatorName} must be a number", Position);
        }

        protected override string OperatorName => nameof(Date);

        public override TypedValue GetValue(List<TypedValue> ArgumentValues, Dictionary<string, int> IterationIndexes, string TimeZone) =>
            base.GetValue(ArgumentValues, IterationIndexes, TimeZone) ??
            new TypedValue(DataType.Date, new DateTimeOffset((int)ArgumentValues[0].NumberValue, (int)ArgumentValues[1].NumberValue, (int)ArgumentValues[2].NumberValue, 0, 0, 0, TimeSpan.Zero));

        protected override DataType GetDataType() => DataType.Date;
    }
}
