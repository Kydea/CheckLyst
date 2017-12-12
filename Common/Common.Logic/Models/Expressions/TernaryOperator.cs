using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class TernaryOperator : Symbol
    {
        public Symbol Condition { get; set; }

        public Symbol TruePart { get; set; }

        public Symbol FalsePart { get; set; }

        public override Expression Expression
        {
            set
            {
                Condition.Expression = TruePart.Expression = FalsePart.Expression = base.Expression = value;
            }
        }

        public override void Check(ICheckList Prototype)
        {
            Condition.Check(Prototype);
            TruePart.Check(Prototype);
            FalsePart.Check(Prototype);

            if (Condition.DataType != DataType.Boolean) throw new Exception("The condition must be a boolean");
            if (TruePart.DataType != FalsePart.DataType) throw new Exception("The 'true part' and 'false part' must implement the same type");

            DataType = TruePart.DataType;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch)
        {
            var ConditionValue = Condition.GetValue(ElementsBranch);
            var TrueValue = TruePart.GetValue(ElementsBranch);
            var FalseValue = FalsePart.GetValue(ElementsBranch);
            if (ConditionValue == null || TrueValue == null || FalseValue == null) throw new NullReferenceException("Software design error");
            if (ConditionValue.Type == DataType.Error) return ConditionValue;
            if (TrueValue.Type == DataType.Error) return TrueValue;
            if (FalseValue.Type == DataType.Error) return FalseValue;

            return !ConditionValue.IsNull && ConditionValue.BooleanValue ? TrueValue : FalseValue;
        }

        public override string ToString()
        {
            return $"({Condition?.ToString() ?? "#"} ? {TruePart?.ToString() ?? "#"} : {FalsePart?.ToString() ?? "#"})";
        }
    }
}
