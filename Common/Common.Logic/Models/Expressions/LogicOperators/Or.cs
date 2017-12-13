namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public class Or : BiaryLogicOperator
    {
        protected override TypedValue Evaluate(TypedValue leftValue, TypedValue rightValue)
        {
            return base.Evaluate(leftValue, rightValue) ?? new TypedValue(DataType.Boolean, !leftValue.IsNull && leftValue.BooleanValue || !rightValue.IsNull && rightValue.BooleanValue);
        }

        protected override string OperatorName => "or";
    }
}
