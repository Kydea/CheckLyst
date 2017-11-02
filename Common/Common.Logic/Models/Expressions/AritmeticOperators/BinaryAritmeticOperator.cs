using System;

namespace CheckListManager.Common.Logic.Models.Expressions.AritmeticOperators
{
	public abstract class BinaryAritmeticOperator : BinaryOperator
	{
		protected abstract TypedValue EvaluateResult(TypedValue leftValue, TypedValue rightValue);

		protected override TypedValue Evaluate(TypedValue leftValue, TypedValue rightValue)
		{
			var value = base.Evaluate(leftValue, rightValue);
			if (value == null)
				try
				{
					return EvaluateResult(leftValue, rightValue);
				}
				catch (Exception exception)
				{
					return new TypedValue(DataType.Error, exception.Message);
				}

			return value;
		}
	}
}
