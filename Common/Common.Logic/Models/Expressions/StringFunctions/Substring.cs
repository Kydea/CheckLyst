using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class Substring : EnnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            if (Arguments.Count != 3 || Arguments.Any(a => a == null)) throw new NullReferenceException("Software design error");

            base.Check(CheckList, Expression, IteratorStack);
            if (Arguments[0].DataType != DataType.Text) throw new ParseException($"The first argument of {OperatorName} must be a text", Position);
            if (Arguments[1].DataType != DataType.Number) throw new ParseException($"The second argument of {OperatorName} must be a number", Position);
            if (Arguments[2].DataType != DataType.Number) throw new ParseException($"The third argument of {OperatorName} must be a number", Position);
        }

        protected override DataType GetDataType() => DataType.Text;

        protected override string OperatorName => "Substring";

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
                var InitialPosition = (int)ArgumentValues[1].NumberValue;
                if (InitialPosition > Text.Length) return new TypedValue(DataType.Text, string.Empty);
                var Length = ArgumentValues[2].IsNull ? default(int?) : (int)ArgumentValues[2].NumberValue;
                if (Length > Text.Length - InitialPosition) Length = null;
                Result = new TypedValue(
                    DataType.Text,
                    Length.HasValue ?
                        Text.Substring(InitialPosition, Length.Value) :
                        Text.Substring(InitialPosition)
                );
            }
            return Result;
        }
    }
}
