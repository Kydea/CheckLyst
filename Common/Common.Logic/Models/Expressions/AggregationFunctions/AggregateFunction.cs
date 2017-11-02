using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.AggregationFunctions
{
    public abstract class AggregateFunction : UnaryOperator
    {
	    protected AggregateFunction(string iteratorCode)
        {
            IteratorCode = iteratorCode;
        }

        public override void Check(Prototype prototype)
        {
            base.Check(prototype);

            var element = prototype.Elements[IteratorCode];
            if (!(element is Group)) throw new Exception("The element with code " + IteratorCode + " is not a Group");
            if (!((Group)element).Iterator) throw new Exception("The group " + IteratorCode + " is not defined as Iterator");
        }

        protected string IteratorCode { get; }

        protected abstract TypedValue GetValue(IEnumerable<TypedValue> values);

        public override TypedValue GetValue(IElementsBranch elementsBranch)
        {
            var values = ((Group)elementsBranch.Elements[IteratorCode])
				.Iterations
				.Select(iteration => Argument.GetValue(iteration))
				.ToList();
            return values.FirstOrDefault(typedValue => typedValue.Type == DataType.Error) ?? GetValue(values);
        }

		public static AggregateFunction Parse(MethodCallExpression expression, ICheckList checkList)
		{
			if (expression.Method.Name == nameof(Sum))
				return new Sum(((ParameterExpression)expression.Arguments[0]).Name)
				{
					Argument = Symbol.Parse(expression.Arguments[1], checkList)
				};

			throw new Exception($"Method call {expression.Method.Name} not managed");
		}
	}
}
