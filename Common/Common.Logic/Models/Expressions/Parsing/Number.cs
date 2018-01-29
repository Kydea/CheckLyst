using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Number : Token
    {
        public Number(ref string ExpressionValue, ref int Position, int Index) :
            base
            (
                ref ExpressionValue, ref Position, Index, (c, i) =>
                    c >= '0' && c <= '9'
                    || c == '.'
                    || (c == '+' || c == '-') && i == 0
            )
        {
            decimal DecimalValue;
            if (!decimal.TryParse(Value, out DecimalValue))
                throw new ParseException("Invalid number: " + Value, Position);

            Symbol = new Constant(new TypedValue(DataType.Number, DecimalValue)) { Position = Position };
            Operator = false;
        }
    }

}