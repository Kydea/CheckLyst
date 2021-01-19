using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class IterationValue : BinaryIteratorFunction
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            var Element = ElementProperties.ElementProperty.Check(CheckList, IteratorStack, IteratorCode, Position);
            if (!(Element is ICheckListGroup Group)) throw new ParseException("The element with code " + IteratorCode + " is not a Group", Position);
            if (!Group.Iterator) throw new ParseException("The group " + IteratorCode + " is not an Iterator", Position);

            Left.Check(CheckList, Expression, IteratorStack);

            var IteratorIndex = IteratorStack.IndexOf(IteratorCode);
            IteratorStack.Add(IteratorCode);
            Right.Check(CheckList, Expression, IteratorStack);
            IteratorStack.RemoveAt(IteratorStack.Count - 1);

            if (Left.DataType != DataType.Number)
                throw new ParseException("The first operand of the function IterationValue must be a number", Position);
            DataType = Right.DataType;
        }

        protected override TypedValue GetValue(List<TypedValue> LeftValues, List<TypedValue> RightValues) => throw new NotImplementedException();

        public override TypedValue GetValue(IElementsBranch ElementsBranch, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            var Iterator = (ICheckListGroup)ElementsBranch.Elements[IteratorCode];
            var LeftValue = Left.GetValue(ElementsBranch, IterationIndexes, TimeZone);
            if (LeftValue.Type == DataType.Error || LeftValue.IsNull) return LeftValue;
            var Index = LeftValue.NumberValue - 1;

            var RightValues = GetIteratorValues(Iterator, Right, IterationIndexes, TimeZone);
            return
                Math.Truncate(Index) != Index || Index < 0 || Index >= RightValues.Count ? new TypedValue(DataType.Number, null) :
                RightValues[(int)Index];
        }

        protected override string OperatorName => "IterationValue";
    }
}
