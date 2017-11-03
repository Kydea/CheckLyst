using System;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.AritmeticOperators;
using CheckListManager.Common.Logic.Models.Expressions.ComparisionOperators;
using CheckListManager.Common.Logic.Models.Expressions.LogicOperators;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class BinaryOperator : Symbol
    {
        public Symbol Left { get; set; }

        public Symbol Right { get; set; }

        public override Expression Expression
        {
            set => Left.Expression = Right.Expression = base.Expression = value;
        }

        public override void Check(Prototype prototype)
        {
            Left.Check(prototype);
            Right.Check(prototype);
        }

        protected virtual TypedValue Evaluate(TypedValue leftValue, TypedValue rightValue)
        {
            return
                leftValue.Type == DataType.Error ? leftValue :
                rightValue.Type == DataType.Error ? rightValue :
                null;
        }

        public override TypedValue GetValue(IElementsBranch elementsBranch)
        {
            var value = Evaluate(Left.GetValue(elementsBranch), Right.GetValue(elementsBranch));
            if (value == null) throw new NullReferenceException("Software design error");
            return value;
        }

		public static BinaryOperator Parse(BinaryExpression expression, ICheckList checkList)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.Add:
					return new Plus { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.Subtract:
					return new Minus { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.Multiply:
					return new Times { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.Divide:
					return new Division { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.Equal:
					return new Equals { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.GreaterThan:
					return new GreaterThen { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.GreaterThanOrEqual:
					return new GreaterOrEqual { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.LessThan:
					return new LessThen { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.LessThanOrEqual:
					return new LessOrEqual { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.Or:
				case ExpressionType.OrElse:
					return new Or { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
				case ExpressionType.And:
				case ExpressionType.AndAlso:
					return new And { Left = Symbol.Parse(expression.Left, checkList), Right = Symbol.Parse(expression.Right, checkList) };
			}

			throw new Exception($"Node Type {expression.NodeType.ToString()} not managed");
		}
	}
}
