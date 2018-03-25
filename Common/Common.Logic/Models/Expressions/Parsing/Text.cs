using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Text : Token
    {
        public Text(ref string ExpressionValue, ref int Position, int Index, char Separator) : base(Position, Index)
        {
            while (Position == this.Position || ExpressionValue[Position] != Separator)
                if (++Position == ExpressionValue.Length)
                    throw new ParseException(Separator + " expected", Position);

            Value = ExpressionValue.Substring(this.Position + 1, Position++ - this.Position - 1);
            Symbol = new Constant(new TypedValue(DataType.Text, Value)) { Position = Position };
            Operator = OperatorType.No;
        }
    }
}