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

            //extracts the peace where the iteration is
            var IteratorIndex = IteratorStack.IndexOf(IteratorCode);
            var NewIteratorkStack = IteratorStack.GetRange(0, IteratorIndex + 1);
            Right.Check(CheckList, Expression, NewIteratorkStack);

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
            var Index = LeftValue.NumberValue;

            //the second arguments create a new iterator context
            var RightValues = GetIteratorValues(Iterator, Right, new Dictionary<string, int>(), TimeZone);
            return
                Math.Truncate(Index) != Index || Index < 0 || Index >= RightValues.Count ? new TypedValue(DataType.Number, null) :
                RightValues[(int)Index];
        }

        protected override string OperatorName => "IterationValue";
    }
}
