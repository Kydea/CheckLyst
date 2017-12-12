using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ComparisonOperators
{
    public abstract class BinaryComparisonOperator : BinaryOperator
    {
        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);

            if (
                Left.DataType != Right.DataType
                && !(
                        (Left.DataType == DataType.Date && Right.DataType == DataType.DateTime)
                        || (Left.DataType == DataType.DateTime && Right.DataType == DataType.Date)
                    )
            )
                throw new Exception("Comparison operators operands have the same type");

            DataType = DataType.Boolean;
        }

        protected override TypedValue Evaluate(TypedValue LeftValue, TypedValue RightValue)
        {
            return
                base.Evaluate(LeftValue, RightValue) ?? (LeftValue.IsNull || RightValue.IsNull ? new TypedValue(DataType.Boolean, null) : null);
        }
    }
}
