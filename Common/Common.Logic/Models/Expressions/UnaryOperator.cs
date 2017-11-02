using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class UnaryOperator : Symbol
    {
        public Symbol Argument { get; set; }

        public override Expression Expression
        {
            set => Argument.Expression = base.Expression = value;
        }

        public override void Check(Prototype prototype)
        {
            Argument.Check(prototype);
            DataType = Argument.DataType;
        }

        protected virtual TypedValue Evaluate(TypedValue argumentValue)
        {
            return argumentValue.Type == DataType.Error ? argumentValue : null;
        }

        public override TypedValue GetValue(IElementsBranch elementsBranch)
        {
            var value = Evaluate(Argument.GetValue(elementsBranch));
            if (value == null) throw new NullReferenceException("Software design error");
            return value;
        }
    }
}
