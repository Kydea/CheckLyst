using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public abstract class UnaryIteratorFunction : UnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            var Element = ElementProperties.ElementProperty.Check(CheckList, IteratorStack, IteratorCode, Position);
            if (!(Element is ICheckListGroup)) throw new ParseException("The element with code " + IteratorCode + " is not a Group", Position);
            if (!((ICheckListGroup)Element).Iterator) throw new ParseException("The group " + IteratorCode + " is not an Iterator", Position);

            IteratorStack.Add(IteratorCode);
            base.Check(CheckList, Expression, IteratorStack);
            IteratorStack.RemoveAt(IteratorStack.Count - 1);
        }

        public string IteratorCode { get; set; }

        protected abstract TypedValue GetValue(List<TypedValue> Values);

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var Iterator = (ICheckListGroup)ElementsBranch.Elements[IteratorCode];
            var Values = Iterator.Iterations.Where(x => !x.Removed).Select(Iteration => Argument.GetValue(Iteration, TimeZone)).ToList();
            return Values.FirstOrDefault(TypedValue => TypedValue.Type == DataType.Error) ?? GetValue(Values);
        }
    }
}
