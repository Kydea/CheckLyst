namespace CheckListManager.Common.Logic.Models.Expressions.LogicOperators
{
    public class And : BiaryLogicOperator
    {
        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue, string TimeZone)
        {
            return base.Evaluate(LeftValue, RightValue, TimeZone) ?? new TypedValue(DataType.Boolean, !LeftValue.IsNull && LeftValue.BooleanValue && !RightValue.IsNull && RightValue.BooleanValue);
        }

        protected override string OperatorName => "and";
    }
}
