using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class TokenSet : Token, IComparer<Token>
    {
        public TokenSet(string ExpressionString, int Position, int? EndIndex = null, TokenSet Parent = null, int? Index = null) : base(Position, Index ?? -1)
        {
            Operator = OperatorType.No;
            Tokens = new List<Token>();
            TokenIndexes = new SortedList<Token, int>(this);

            //Token parse and functions connection
            var ExpressionValue = ExpressionString ?? string.Empty;
            var Caret = Position;
            if (!EndIndex.HasValue) EndIndex = ExpressionValue.Length;

            Token LastToken = null;

            while (Caret < EndIndex)
            {
                Token Token = null;
                var Character = ExpressionString[Caret];
                if (Char.IsWhiteSpace(Character))
                    Caret++;
                else if (Character == '(')
                    Add(Token = ParseParenteses(ref ExpressionValue, ref Caret, '(', ')'));
                else if (Character == '?')
                {
                    Add(Token = new TernaryOperator(ref Caret, Tokens.Count));
                    Add(ParseParenteses(ref ExpressionValue, ref Caret, '?', ':'));
                }
                else if (Character >= 'a' && Character <= 'z' || Character >= 'A' && Character <= 'Z')
                    Add(Token = new Literal(ref ExpressionValue, ref Caret, Tokens.Count));
                else if (
                   Character >= '0' && Character <= '9'
                   || Character == '.'
                   || (Character == '+' || Character == '-') && !Tokens.Any()
                )
                    Add(Token = new Number(ref ExpressionValue, ref Caret, Tokens.Count));
                else if (Parsing.Operator.OPERATORS_CHARACTERS.Contains(Character.ToString()))
                    Add(Token = new Operator(ref ExpressionValue, ref Caret, Tokens.Count));
                else if (Character == '\'' || Character == '"')
                    Add(LastToken = new Text(ref ExpressionValue, ref Caret, Tokens.Count, Character));
                else
                    throw new ParseException("Invalid character " + Character, Caret);

                if (Token != null)
                {
                    if (LastToken != null)
                    {
                        if ((Token.Operator == OperatorType.Left || Token.Operator == OperatorType.Both) && (LastToken.Operator == OperatorType.Right || LastToken.Operator == OperatorType.Both))
                            throw new ParseException("Unexpected token " + Token.Value, Caret);

                        if ((Token.Operator == OperatorType.Right || Token.Operator == OperatorType.No) && (LastToken.Operator == OperatorType.Left || LastToken.Operator == OperatorType.No))
                            throw new ParseException("Operator expected", Token.Position);
                    }

                    LastToken = Token;
                }
            }

            //Expression symbol connection
            var TokenIndex = 0;
            while (TokenIndex < TokenIndexes.Count())
            {
                var Token = TokenIndexes.ElementAt(TokenIndex).Key;
                if (Token is TokenSet || Token is Text || Token is Number || !Tokens.Contains(Token))
                {
                    ++TokenIndex;
                    continue;
                }

                bool Operator = false;

                if (Token.Symbol is UnaryOperator && Token.Symbol is LogicOperators.Not)
                {
                    var UnaryOperator = (UnaryOperator)Token.Symbol;
                    if (Token.Index == Tokens.Count - 1)
                        throw new ParseException("Expected operand", ExpressionValue.Length);

                    var Operand = Tokens[Token.Index + 1];
                    UnaryOperator.Argument = Operand.Symbol;
                    Tokens.RemoveAt(Token.Index + 1);
                    Operator = true;
                }
                else if (Token.Symbol is BinaryOperator)
                {
                    var BinaryOperator = (BinaryOperator)Token.Symbol;
                    if (Token.Index == 0)
                        throw new ParseException("Expected operand", Token.Position);
                    BinaryOperator.Left = Tokens[Token.Index - 1].Symbol;

                    if (Token.Index == Tokens.Count - 1)
                        throw new ParseException("Expected operand", ExpressionValue.Length);
                    BinaryOperator.Right = Tokens[Token.Index + 1].Symbol;

                    Tokens.RemoveAt(Token.Index + 1);
                    Tokens.RemoveAt(Token.Index - 1);
                    Operator = true;
                }
                else if (Token.Symbol is Expressions.TernaryOperator && Token.Value == null) //iif operator is a function and then it is already connected
                {
                    var TernaryOperator = (Expressions.TernaryOperator)Token.Symbol;
                    if (Token.Index == 0)
                        throw new ParseException("Expected operand", Token.Position);
                    TernaryOperator.Condition = Tokens[Token.Index - 1].Symbol;

                    if (Token.Index == Tokens.Count - 1)
                        throw new ParseException("Expected operand", Tokens[Token.Index + 1].Position);
                    TernaryOperator.TruePart = Tokens[Token.Index + 1].Symbol;

                    if (Token.Index == Tokens.Count - 2)
                        throw new ParseException("Expected operand", Tokens[Token.Index + 2].Position);
                    TernaryOperator.FalsePart = Tokens[Token.Index + 2].Symbol;

                    Tokens.RemoveAt(Token.Index + 2);
                    Tokens.RemoveAt(Token.Index + 1);
                    Tokens.RemoveAt(Token.Index - 1);
                    Operator = true;
                }
                else if (Token is Literal)
                {
                    var Literal = (Literal)Token;

                }

                Symbol = Token.Symbol;

                if (Operator)
                    for (int Order = 0; Order < Tokens.Count; ++Order)
                        Tokens[Order].Index = Order;

                ++TokenIndex;
            }

            if (Symbol == null && Tokens.Any())
                Symbol = Tokens[0].Symbol;
        }

        private TokenSet ParseParenteses(ref string ExpressionValue, ref int Position, char LeftParenteses, char RightParenteses)
        {
            int
                Caret = Position + 1,
                Depth = 1;

            while (Caret < ExpressionValue.Length && Depth > 0)
            {
                var Character = ExpressionValue[Caret];
                if (Character == LeftParenteses)
                    ++Depth;
                else if (Character == RightParenteses)
                    --Depth;

                ++Caret;
            }

            if (Depth > 0) throw new ParseException(RightParenteses + " expected", Caret);

            var TokenSet = new TokenSet(ExpressionValue, Position + 1, Caret - 1, this, Tokens.Count);
            Position = Caret;

            return TokenSet;
        }

        private List<Token> Tokens { get; set; }

        private SortedList<Token, int> TokenIndexes { get; }

        public int Compare(Token Token1, Token Token2)
        {
            return
                (Token1.TokenOrder - Token2.TokenOrder) * 1000
                + (Token1.TokenOrderRightToLeft ? Token2.Position - Token1.Position : Token1.Position - Token2.Position);
        }

        private void Add(Token Token)
        {
            TokenIndexes.Add(Token, Tokens.Count());
            Tokens.Add(Token);
        }

        public override string ToString()
        {
            return $"({string.Join(", ", TokenIndexes.Select(t => t.Key.ToString()))})";
        }
    }
}