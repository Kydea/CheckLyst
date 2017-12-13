using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public abstract class Token
    {
        protected Token(int Position, int Index)
        {
            this.Position = Position;
            this.Index = Index;
        }

        protected Token(ref string ExpressionValue, ref int Position, int Index, Func<char, bool> Test) : this(Position, Index)
        {
            while (Position < ExpressionValue.Length && Test(ExpressionValue[Position]))
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
                    Value == "?" ? 8 : //Conditional (this value is overridden by the specific class)
                    Value == "||" || Value == "or" ? 7 : //Conditional or
                    Value == "&&" || Value == "and" ? 6 : //Conditional and
                    Value == "==" || Value == "!=" || Value == "=" ? 5 : //Equality
                    Value.StartsWith("<") || Value.StartsWith(">") ? 4 : //Relational
                    Value == "+" || Value == "-" ? 3 : //Additive
                    Value == "*" || Value == "/" || Value == "%" ? 2 : //Multiplicative
                    this.Value != null ? 1 : //literal and numbers
                    0; //Paranteses (Token set) and unexpected tokens,
            }
        }

        public virtual bool TokenOrderRightToLeft { get; private set; }

        public Symbol Symbol { get; protected set; }

        public string Value { get; protected set; }

        public int Position { get; private set; }

        public int Index { get; set; }

        public bool Operator { get; protected set; } = true;
    }

}