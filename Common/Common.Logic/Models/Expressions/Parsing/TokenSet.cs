using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class TokenSet : Token, IComparer<Token>
    {
        public TokenSet(string ExpressionString, Position Position, int? EndIndex = null, TokenSet Parent = null, int? Index = null) : base(Position, Index ?? -1)
        {
            Operator = OperatorType.No;
            Tokens = new List<Token>();
            TokenIndexes = new SortedList<Token, int>(this);

            //Token parse and functions connection
            var ExpressionValue = ExpressionString ?? string.Empty;
            if (!EndIndex.HasValue) EndIndex = ExpressionValue.Length;

            Token LastToken = null;

            while (Position.Index < EndIndex)
            {
                Token Token = null;
                var Character = ExpressionString[Position.Index];
                if (char.IsWhiteSpace(Character))
                    Position.Increment(Character == '\n');
                else if (Character == '(')
                    Add(Token = ParseParenteses(ref ExpressionValue, Position, '(', ')'));
                else if (Character == '?')
                {
                    Add(Token = new ConditionalOperator(Position, Tokens.Count));
                    Add(ParseParenteses(ref ExpressionValue, Position, '?', ':'));
                }
                else if (Character >= 'a' && Character <= 'z' || Character >= 'A' && Character <= 'Z')
                    Add(Token = new Literal(ref ExpressionValue, Position, Tokens.Count));
                else if (
                   Character >= '0' && Character <= '9'
                   || Character == '.'
                   || (Character == '+' || Character == '-') && !Tokens.Any()
                )
                    Add(Token = new Number(ref ExpressionValue, Position, Tokens.Count));
                else if (Parsing.Operator.OPERATORS_CHARACTERS.Contains(Character.ToString()))
                    Add(Token = new Operator(ref ExpressionValue, Position, Tokens.Count));
                else if (Character == '\'' || Character == '"')
                    Add(LastToken = new Text(ref ExpressionValue, Position, Tokens.Count, Character));
                else
                    throw new ParseException("Invalid character " + Character, Position);

                if (Token != null)
                {
                    if (LastToken != null)
                    {
                        if ((Token.Operator == OperatorType.Left || Token.Operator == OperatorType.Both) && (LastToken.Operator == OperatorType.Right || LastToken.Operator == OperatorType.Both))
                            throw new ParseException("Unexpected token " + Token.Value, Position);

                        if ((Token.Operator == OperatorType.Right || Token.Operator == OperatorType.No) && (LastToken.Operator == OperatorType.Left || LastToken.Operator == OperatorType.No))
                            throw new ParseException("Operator expected", Token.Position);
                    }

                    LastToken = Token;
                }
            }

            //Operators (including literals with no arguments) connection
            var TokenIndex = 0;
            while (TokenIndex < TokenIndexes.Count())
            {
                var Token = TokenIndexes.ElementAt(TokenIndex).Key;

                if (Token.Operator == OperatorType.No || !Tokens.Contains(Token))
                {
                    ++TokenIndex;
                    continue;
                }

                if (Token.Symbol is UnaryOperator UnaryOperator)
                {
                    if (Token.Index == Tokens.Count - 1)
                        throw new ParseException("Expected operand", Position);

                    var Operand = Tokens[Token.Index + 1];
                    UnaryOperator.Argument = Operand.Symbol;
                    Tokens.RemoveAt(Token.Index + 1);
                }
                else if (Token.Symbol is BinaryOperator BinaryOperator)
                {
                    if (Token.Index == 0)
                        throw new ParseException("Expected operand", Token.Position);
                    BinaryOperator.Left = Tokens[Token.Index - 1].Symbol;

                    if (Token.Index == Tokens.Count - 1)
                        throw new ParseException("Expected operand", Position);
                    BinaryOperator.Right = Tokens[Token.Index + 1].Symbol;

                    Tokens.RemoveAt(Token.Index + 1);
                    Tokens.RemoveAt(Token.Index - 1);
                }
                else if (Token.Symbol is Expressions.ConditionalOperator ConditionalOperator)
                {
                    if (Token.Index == 0)
                        throw new ParseException("Expected operand", Token.Position);
                    ConditionalOperator.Condition = Tokens[Token.Index - 1].Symbol;

                    if (Token.Index == Tokens.Count - 1)
                        throw new ParseException("Expected operand", Tokens[Token.Index + 1].Position);
                    ConditionalOperator.TruePart = Tokens[Token.Index + 1].Symbol;

                    if (Token.Index == Tokens.Count - 2)
                        throw new ParseException("Expected operand", Tokens[Token.Index + 2].Position);
                    ConditionalOperator.FalsePart = Tokens[Token.Index + 2].Symbol;

                    Tokens.RemoveAt(Token.Index + 2);
                    Tokens.RemoveAt(Token.Index + 1);
                    Tokens.RemoveAt(Token.Index - 1);
                }
                else
                    throw new ParseException($"Unexpected operator {Token.ToString()}", Position);

                Symbol = Token.Symbol;

                for (int Order = 0; Order < Tokens.Count; ++Order)
                    Tokens[Order].Index = Order;

                ++TokenIndex;
            }

            if (Symbol == null && Tokens.Any())
                Symbol = Tokens[0].Symbol;
        }

        private TokenSet ParseParenteses(ref string ExpressionValue, Position Position, char LeftParenteses, char RightParenteses)
        {
            Position.Increment(false);
            int
                Caret = Position.Index,
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
                
            if (Depth > 0) throw new ParseException($"{RightParenteses} expected", Position);

            var TokenSet = new TokenSet(ExpressionValue, Position, Caret - 1, this, Tokens.Count);
            Position.Increment(false);

            return TokenSet;
        }

        private List<Token> Tokens { get; set; }

        private SortedList<Token, int> TokenIndexes { get; }

        public int Compare(Token Token1, Token Token2)
        {
            return
                (Token1.TokenOrder - Token2.TokenOrder) * Expression.MAXIMUM_LENGTH
                + (Token1.TokenOrderRightToLeft ? Token2.Position.Index - Token1.Position.Index : Token1.Position.Index - Token2.Position.Index);
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