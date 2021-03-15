using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class DateTimeIncrement : EnnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            if (Arguments.Count != 6 || Arguments.Any(a => a == null)) throw new NullReferenceException("Software design error");
            if (Arguments[0].DataType != DataType.Date && Arguments[0].DataType != DataType.DateTime) throw new ParseException($"The first argument of {OperatorName} must be a date or a date/time", Position);
            if (Arguments[1].DataType != DataType.Number) throw new ParseException($"The second argument of {OperatorName} must be a number", Position);
            if (Arguments[2].DataType != DataType.Number) throw new ParseException($"The third argument of {OperatorName} must be a number", Position);
            if (Arguments[3].DataType != DataType.Number) throw new ParseException($"The fourth argument of {OperatorName} must be a number", Position);
            if (Arguments[4].DataType != DataType.Number) throw new ParseException($"The fifth argument of {OperatorName} must be a number", Position);
            if (Arguments[5].DataType != DataType.Number) throw new ParseException($"The sixth argument of {OperatorName} must be a number", Position);

            base.Check(CheckList, Expression, IteratorStack);
        }

        protected override DataType GetDataType() => DataType.Date;

        protected override string OperatorName => "DateTimeIncrement";

        public override TypedValue GetValue(List<TypedValue> ArgumentValues, Dictionary<string, int> IterationIndexes, string TimeZone) =>
            base.GetValue(ArgumentValues, IterationIndexes, TimeZone) ??
            new TypedValue(
                DataType.DateTime,
                ArgumentValues[0].DateTimeValue
                    .AddYears((int)ArgumentValues[1].NumberValue)
                    .AddMonths((int)ArgumentValues[2].NumberValue)
                    .AddDays((int)ArgumentValues[3].NumberValue)
                    .AddHours((int)ArgumentValues[3].NumberValue)
                    .AddMinutes((int)ArgumentValues[3].NumberValue)
            );
    }
}
