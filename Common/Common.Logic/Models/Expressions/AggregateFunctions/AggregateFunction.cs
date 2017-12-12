using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AggregateFunctions
{
    public abstract class AggregateFunction : UnaryOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            var Element = Prototype.Elements[IteratorCode];
            if (!(Element is ICheckListGroup)) throw new Exception("The element with code " + IteratorCode + " is not a Group");
            if (!((ICheckListGroup)Element).Iterator) throw new Exception("The group " + IteratorCode + " is not an Iterator");
        }

        public String IteratorCode { get; set; }

        protected abstract TypedValue GetValue(IEnumerable<TypedValue> Values);

        public override TypedValue GetValue(IElementsBranch ElementsBranch)
        {
            var Values = ((ICheckListGroup)ElementsBranch.Elements[IteratorCode]).Iterations.Select(Iteration => Argument.GetValue(Iteration));
            return Values.FirstOrDefault(TypedValue => TypedValue.Type == DataType.Error) ?? GetValue(Values);
        }
    }
}
