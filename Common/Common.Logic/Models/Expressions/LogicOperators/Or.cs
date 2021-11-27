namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public class Or : BiaryLogicOperator
    {
        protected override TypedValue Evaluate(TypedValue leftValue, TypedValue rightValue, string TimeZone)
        {
            return base.Evaluate(leftValue, rightValue, TimeZone) ?? new TypedValue(DataType.Boolean, !leftValue.IsNull && leftValue.BooleanValue || !rightValue.IsNull && rightValue.BooleanValue);
        }

        protected override string OperatorName => "or";
    }
}
