using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Number : Token
    {
        public Number(ref string ExpressionValue, Position Position, int Index) :
            base
            (
                ref ExpressionValue, Position, Index, (c, i) =>
                    c >= '0' && c <= '9'
                    || c == '.'
                    || (c == '+' || c == '-') && i == 0
            )
        {
            if (!decimal.TryParse(Value, out decimal DecimalValue))
                throw new ParseException("Invalid number: " + Value, Position);

            Symbol = new Constant(new TypedValue(DataType.Number, DecimalValue)) { Position = Position };
            Operator = OperatorType.No;
        }
    }

}