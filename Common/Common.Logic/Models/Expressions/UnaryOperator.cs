using System;
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

        public override void Check(ICheckList Prototype)
        {
            Argument.Check(Prototype);
            DataType = Argument.DataType;
        }

        protected virtual TypedValue Evaluate(TypedValue ArgumentValue)
        {
            return ArgumentValue.Type == DataType.Error ? ArgumentValue : null;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch)
        {
            var Value = Evaluate(Argument.GetValue(ElementsBranch));
            if (Value == null) throw new NullReferenceException("Software design error");
            return Value;
        }
    }
}
