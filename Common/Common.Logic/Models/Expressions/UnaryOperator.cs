using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class UnaryOperator : Symbol
    {
        public Symbol Argument { get; set; }

        public override Expression Expression
        {
            set
            {
                Argument.Expression = base.Expression = value;
            }
        }

        public override void Check(ICheckList Prototype, Expression Expression, List<string> IteratorStack)
        {
            Argument.Check(Prototype, Expression, IteratorStack);
            DataType = Argument.DataType;
        }

        protected virtual TypedValue Evaluate(TypedValue ArgumentValue)
        {
            return ArgumentValue.Type == DataType.Error ? ArgumentValue : null;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var Value = Evaluate(Argument.GetValue(ElementsBranch, TimeZone));
            if (Value == null) throw new NullReferenceException("Software design error");
            return Value;
        }
    }
}
