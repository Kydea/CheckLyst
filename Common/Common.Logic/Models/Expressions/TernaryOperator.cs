using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

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

        public override void Check(ICheckList Prototype, Expression Expression, List<string> IteratorStack)
        {
            Condition.Check(Prototype, Expression, IteratorStack);
            TruePart.Check(Prototype, Expression, IteratorStack);
            FalsePart.Check(Prototype, Expression, IteratorStack);

            if (Condition.DataType != DataType.Boolean) throw new ParseException("The condition must be a boolean", Position);
            if (TruePart.DataType != FalsePart.DataType) throw new ParseException("The 'true part' and 'false part' must implement the same type", Position);

            DataType = TruePart.DataType;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var ConditionValue = Condition.GetValue(ElementsBranch, TimeZone);
            var TrueValue = TruePart.GetValue(ElementsBranch, TimeZone);
            var FalseValue = FalsePart.GetValue(ElementsBranch, TimeZone);
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
