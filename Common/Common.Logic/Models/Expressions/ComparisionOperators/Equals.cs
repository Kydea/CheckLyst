namespace CheckListManager.Common.Logic.Models.Expressions.ComparisionOperators
{
    public class Equals : BinaryComparisonOperator
    {
        protected override TypedValue Evaluate(TypedValue leftValue, TypedValue rightValue)
        {
            return base.Evaluate(leftValue, rightValue) ?? new TypedValue(DataType.Boolean, Equals(leftValue.Value, rightValue.Value));
        }
    }
}
