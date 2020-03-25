using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class BinaryOperator : Symbol
    {
        public Symbol Left { get; set; }

        public Symbol Right { get; set; }

        public override Expression Expression
        {
            set
            {
                Left.Expression = Right.Expression = base.Expression = value;
            }
        }

        public override void BuildDependencies(ICheckList CheckList, Expression Expression)
        {
            Left.BuildDependencies(CheckList, Expression);
            Right.BuildDependencies(CheckList, Expression);
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            Left.Check(CheckList, Expression, IteratorStack);
            Right.Check(CheckList, Expression, IteratorStack);
        }

        protected virtual TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue)
        {
            return
                LeftValue.Type == DataType.Error ? LeftValue :
                RightValue.Type == DataType.Error ? RightValue :
                null;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var Value = Evaluate(Left.GetValue(ElementsBranch, TimeZone), Right.GetValue(ElementsBranch, TimeZone));
            if (Value == null) throw new NullReferenceException("Software design error");
            return Value;
        }

        protected abstract string OperatorName { get; }

        public override string ToString()
        {
            return $"({Left?.ToString() ?? "#"} {OperatorName} {Right?.ToString() ?? "#"})";
        }
    }
}
