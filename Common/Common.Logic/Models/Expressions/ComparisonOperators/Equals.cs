using System;

namespace CheckListManager.Common.Logic.Models.Expressions.ComparisonOperators
{
    public class Equals : BinaryComparisonOperator
    {
        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue)
        {
            return base.Evaluate(LeftValue, RightValue) ?? new TypedValue(DataType.Boolean, Object.Equals(LeftValue.Value, RightValue.Value));
        }

        protected override string OperatorName => "==";
    }
}
