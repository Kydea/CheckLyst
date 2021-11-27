using System;

namespace CheckListManager.Common.Logic.Models.Expressions.ComparisonOperators
{
    public class Equals : BinaryComparisonOperator
    {
        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue, string TimeZone)
        {
            return base.Evaluate(LeftValue, RightValue, TimeZone) ?? new TypedValue(DataType.Boolean, Equals(LeftValue.Value, RightValue.Value));
        }

        protected override string OperatorName => "==";
    }
}
