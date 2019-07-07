using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public abstract class Token
    {
        public enum OperatorType { No, Right, Left, Both }

        protected Token(int Position, int Index)
        {
            this.Position = Position;
            this.Index = Index;
        }

        protected Token(ref string ExpressionValue, ref int Position, int Index, Func<char, int, bool> Test) : this(Position, Index)
        {
            var StartPosition = Position;
            while (Position < ExpressionValue.Length && Test(ExpressionValue[Position], Position - StartPosition))
                ++Position;

            Value = ExpressionValue.Substring(this.Position, Position - this.Position);
        }

        public override string ToString()
        {
            return $"{Value ?? ""}";
        }

        public virtual int TokenOrder
        {
            get
            {
                var Value = this.Value?.ToLower() ?? "";
                // - standard order for operators, 
                // - parenteses befor literals to have function numbers before the function name

                return
                    Value == "?" ? 9 : //Conditional (this value is overridden by the specific class)
                    Value == "||" || Value == "or" ? 8 : //Conditional or
                    Value == "&&" || Value == "and" ? 7 : //Conditional and
                    Value == "==" || Value == "!=" || Value == "=" ? 6 : //Equality
                    Value.StartsWith("<") || Value.StartsWith(">") ? 5 : //Relational
                    Value == "+" || Value == "-" ? 4 : //Additive
                    Value == "*" || Value == "/" || Value == "%" ? 3 : //Multiplicative
                    Value == "^" ? 2 : //Multiplicative
                    this.Value != null ? 1 : //literal and numbers
                    0; //Paranteses (Token set) and unexpected tokens,
            }
        }

        public virtual bool TokenOrderRightToLeft { get; protected set; }

        public Symbol Symbol { get; protected set; }

        public string Value { get; protected set; }

        public int Position { get; private set; }

        public int Index { get; set; }

        public OperatorType Operator { get; protected set; } = OperatorType.Both;
    }

}