using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Literal : Token
    {
        public Literal(ref string ExpressionValue, ref int Position, int Index) : base(
            ref ExpressionValue, ref Position, Index, c =>
                c >= 'a' && c <= 'z'
                || c >= 'A' && c <= 'Z'
                || c >= '0' && c <= '9'
                || "àèéìòù_.".Contains(c.ToString())
            )
        {
            string[] Values = Value.Split('.');
            if (Values.Length == 1)
            {
                //Constants
                if (Value.ToLower() == "true")
                {
                    Symbol = new Constant(new TypedValue(DataType.Boolean, true)) { Position = Position };
                    Operator = false;
                }
                else if (Value.ToLower() == "false")
                {
                    Symbol = new Constant(new TypedValue(DataType.Boolean, false)) { Position = Position };
                    Operator = false;
                }

                //Logic operators
                else if (Value.ToLower() == "or")
                    Symbol = new LogicOperators.Or { Position = Position };
                else if (Value.ToLower() == "and")
                    Symbol = new LogicOperators.And { Position = Position };
                else if (Value.ToLower() == "not")
                    Symbol = new LogicOperators.Not { Position = Position };

                //Ternary operator
                else if (Value.ToLower() == "iif")
                {
                    var TernaryOperator = new Expressions.TernaryOperator { Position = Position };
                    ParseArgument(ref ExpressionValue, ref Position, 3);
                    TernaryOperator.Condition = Arguments[0].Symbol;
                    TernaryOperator.TruePart = Arguments[1].Symbol;
                    TernaryOperator.FalsePart = Arguments[2].Symbol;
                    Symbol = TernaryOperator;
                    Operator = false;
                }

                //Expression value
                else
                {
                    Symbol = new ExpressionValue { ExpressionCode = Value, Position = Position };
                    Operator = false;
                }
            }
            else if (Values.Length == 2)
            {
                //Element property
                switch (Values[1])
                {
                    case "Signed":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SignatureProperties.Signed) { Code = Values[0], Position = Position };
                        break;
                    case "SignedBy":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SignatureProperties.SignedBy) { Code = Values[0], Position = Position };
                        break;
                    case "SignedOn":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SignatureProperties.SignedOn) { Code = Values[0], Position = Position };
                        break;
                    case "Value":
                        Symbol = new ElementProperties.FieldProperty { Code = Values[0], Position = Position };
                        break;
                    case "Valid":
                        Symbol = new ElementProperties.ElementProperty(ElementProperties.ElementProperty.ElementProperties.Valid) { Code = Values[0], Position = Position };
                        break;
                    case "Sum":
                        var Sum = new AggregateFunctions.Sum { IteratorCode = Values[0], Position = Position };
                        ParseArgument(ref ExpressionValue, ref Position, 1);
                        Sum.Argument = Arguments[0].Symbol;
                        Symbol = Sum;
                        break;
                    default:
                        throw new ParseException("Invalid property: " + Value, Position);
                }
                Operator = false;
            }
            else
                throw new ParseException("Invalid identifier: " + Value, Position);
        }

        public List<TokenSet> Arguments { get; private set; }

        private void ParseArgument(ref string ExpressionValue, ref int Position, int AcceptedArguments)
        {
            Arguments = new List<TokenSet>();

            int
                Caret = Position,
                ArgumentPosition,
                Depth = 1;

            while (true)
            {
                var Character = ExpressionValue[Caret];
                ++Caret;

                if (Character == '(') break;

                if (!Char.IsWhiteSpace(Character) || Caret == ExpressionValue.Length)
                    throw new ParseException("( expected", Caret);
            }

            ArgumentPosition = Caret;

            while (Caret < ExpressionValue.Length && Depth > 0)
            {
                var Character = ExpressionValue[Caret];
                if (Character == '(')
                    ++Depth;
                else if (Character == ')')
                    --Depth;

                if (Depth == 1 && Character == ',' || Depth == 0)
                {
                    Arguments.Add(new TokenSet(ExpressionValue, ArgumentPosition, Caret));
                    ArgumentPosition = Caret + 1;
                }

                ++Caret;
            }

            if (Depth > 0) throw new ParseException(") expected", Caret);

            Position = Caret + 1;

            if (Arguments.Count() != AcceptedArguments)
                throw new ParseException($"The operator {Value} accets exactly {AcceptedArguments} arguments", this.Position);
        }
    }
}