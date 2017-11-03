using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ComparisionOperators
{
    public abstract class BinaryComparisonOperator : BinaryOperator
    {
        public override void Check(Prototype prototype)
        {
            base.Check(prototype);

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

        protected override TypedValue Evaluate(TypedValue leftValue, TypedValue rightValue)
        {
            return
                base.Evaluate(leftValue, rightValue) ?? (leftValue.IsNull || rightValue.IsNull ? new TypedValue(DataType.Boolean, null) : null);
        }
    }
}
