using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Operator : Token
    {
        public const string OPERATORS_CHARACTERS = "+-*/=?:&|!><";
        public Operator(ref string ExpressionValue, ref int Position, int Index) :
            base
            (
                ref ExpressionValue, ref Position, Index, c =>
                    OPERATORS_CHARACTERS.Contains(c.ToString())
            )
        {
            Symbol =
                //Arithmetic operators
                Value == "+" ? new ArithmeticOperators.Plus() :
                Value == "-" ? new ArithmeticOperators.Minus() :
                Value == "*" ? new ArithmeticOperators.Times() :
                Value == "/" ? new ArithmeticOperators.Division() :

                //Comparision operators
                Value == "==" || Value == "=" ? new ComparisonOperators.Equals() :
                Value == ">=" ? new ComparisonOperators.GreaterOrEqual() :
                Value == ">" ? new ComparisonOperators.GreaterThan() :
                Value == "<=" ? new ComparisonOperators.LessOrEqual() :
                Value == "<" ? new ComparisonOperators.LessThan() :

                //Logic operators
                Value == "||" ? new LogicOperators.Or() :
                Value == "&&" ? new LogicOperators.And() :
                Value == "!" ? new LogicOperators.Not() :

                (Symbol)null;

            if (Symbol == null) throw new ParseException("Invalid operator: " + Value, Position);
            Symbol.Position = Position;
        }
    }

}