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

        public override void BuildDependencies(ICheckList CheckList, Expression Expression)
        {
            Argument.BuildDependencies(CheckList, Expression);
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            Argument.Check(CheckList, Expression, IteratorStack);
            DataType = Argument.DataType;
        }

        protected virtual TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue)
        {
            return ArgumentValue.Type == DataType.Error ? ArgumentValue : null;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            var Value = Evaluate(ElementsBranch, Argument.GetValue(ElementsBranch, IterationIndexes, TimeZone));
            if (Value == null) throw new NullReferenceException("Software design error");
            return Value;
        }
    }
}
