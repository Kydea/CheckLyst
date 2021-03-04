using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Text : Token
    {
        public Text(ref string ExpressionValue, Position Position, int Index, char Separator) : base(Position, Index)
        {
            while (Position.Index == this.Position.Index || ExpressionValue[Position.Index] != Separator)
            {
                Position.Increment(ExpressionValue[Position.Index] == '\n');
                if (Position.Index == ExpressionValue.Length)
                    throw new ParseException($"{Separator} expected", this.Position);
            }

            Value = ExpressionValue.Substring(this.Position.Index + 1, Position.Index - this.Position.Index);
            Position.Increment(false);
            Symbol = new Constant(new TypedValue(DataType.Text, Value)) { Position = Position };
            Operator = OperatorType.No;
        }
    }
}