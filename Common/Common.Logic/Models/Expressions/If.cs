using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class If : Symbol
    {
        public Symbol Condition { get; set; }

        public Symbol TruePart { get; set; }

        public Symbol FalsePart { get; set; }

        public override Expression Expression
        {
            set => Condition.Expression = TruePart.Expression = FalsePart.Expression = base.Expression = value;
        }

        public override void Check(Prototype prototype)
        {
            Condition.Check(prototype);
            TruePart.Check(prototype);
            FalsePart.Check(prototype);

            if (Condition.DataType != DataType.Boolean) throw new Exception("The condition must be a boolean");
            if (TruePart.DataType != FalsePart.DataType) throw new Exception("The 'true part' and 'false part' must implement the same type");

            DataType = TruePart.DataType;
        }

        public override TypedValue GetValue(IElementsBranch elementsBranch)
        {
            var conditionValue = Condition.GetValue(elementsBranch);
            var trueValue = TruePart.GetValue(elementsBranch);
            var falseValue = FalsePart.GetValue(elementsBranch);
            if (conditionValue == null || trueValue == null || falseValue == null) throw new NullReferenceException("Software design error");
            if (conditionValue.Type == DataType.Error) return conditionValue;
            if (trueValue.Type == DataType.Error) return trueValue;
            if (falseValue.Type == DataType.Error) return falseValue;

            return !conditionValue.IsNull && conditionValue.BooleanValue ? trueValue : falseValue; 
        }
	}
}
