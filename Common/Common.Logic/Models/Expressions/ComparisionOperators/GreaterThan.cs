namespace CheckListManager.Common.Logic.Models.Expressions.ComparisionOperators
{
	public class GreaterThen : BinaryComparisonOperator
	{
		protected override TypedValue Evaluate(TypedValue leftValue, TypedValue rightValue)
		{
			return
				base.Evaluate(leftValue, rightValue) ??
				(
					leftValue.Type == DataType.Date || leftValue.Type == DataType.DateTime ?
						new TypedValue(DataType.Boolean, leftValue.DateTimeValue > rightValue.DateTimeValue) :
					leftValue.Type == DataType.Time ?
						new TypedValue(DataType.Boolean, leftValue.TimeSpanValue > rightValue.TimeSpanValue) :
					new TypedValue(DataType.Boolean, leftValue.NumberValue > rightValue.NumberValue)
				);
		}
	}
}
