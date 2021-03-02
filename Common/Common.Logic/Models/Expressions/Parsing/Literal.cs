using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Literal : Token
    {
        private const int MAX_ARGUMENTS = 3;

        public Literal(ref string ExpressionValue, ref int Position, int Index) : base(
            ref ExpressionValue, ref Position, Index, (c, i) =>
                c >= 'a' && c <= 'z'
                || c >= 'A' && c <= 'Z'
                || c >= '0' && c <= '9'
                || "àèéìòù_.".Contains(c.ToString())
            )
        {
            string[] Values = Value.Split('.');
            Arguments = null;
            if (Values.Length == 1)
            {
                var LowerValue = Value.ToLower();
                //Constants
                if (LowerValue == "true")
                {
                    Symbol = new Constant(new TypedValue(DataType.Boolean, true)) { Position = Position };
                    Operator = OperatorType.No;
                }
                else if (LowerValue == "false")
                {
                    Symbol = new Constant(new TypedValue(DataType.Boolean, false)) { Position = Position };
                    Operator = OperatorType.No;
                }

                //Logic operators
                else if (LowerValue == "or")
                    Symbol = new LogicOperators.Or { Position = Position };
                else if (LowerValue == "and")
                    Symbol = new LogicOperators.And { Position = Position };
                else if (LowerValue == "not")
                {
                    Symbol = new LogicOperators.Not { Position = Position };
                    TokenOrderRightToLeft = true;
                    Operator = OperatorType.Right;
                }

                else
                {
                    //Expression value
                    if (!ParseArgument(ref ExpressionValue, ref Position))
                    {
                        Symbol = new ExpressionValue { ExpressionCode = Value, Position = Position };
                        Operator = OperatorType.No;
                    }

                    //functions
                    else if (LowerValue == "error")
                    {
                        var Error = new Functions.Error { Position = Position };
                        CheckArguments(1);
                        Error.Argument = Arguments[0].Symbol;
                        Symbol = Error;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "format")
                    {
                        var Format = new Functions.Format { Position = Position };
                        CheckArguments(2);
                        Format.Left = Arguments[0].Symbol;
                        Format.Right = Arguments[1].Symbol;
                        Symbol = Format;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "null")
                    {
                        var Null = new Functions.Null { Position = Position };
                        CheckArguments(1);
                        Null.Argument = Arguments[0].Symbol;
                        Symbol = Null;
                        Operator = OperatorType.No;
                    }
                    
                    //Math functions
                    else if (LowerValue == "sqrt")
                    {
                        var SqRt = new MathFunctions.SqRt { Position = Position };
                        CheckArguments(1);
                        SqRt.Argument = Arguments[0].Symbol;
                        Symbol = SqRt;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "sin")
                    {
                        var Sin = new MathFunctions.Sin { Position = Position };
                        CheckArguments(1);
                        Sin.Argument = Arguments[0].Symbol;
                        Symbol = Sin;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "cos")
                    {
                        var Cos = new MathFunctions.Cos { Position = Position };
                        CheckArguments(1);
                        Cos.Argument = Arguments[0].Symbol;
                        Symbol = Cos;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "tan")
                    {
                        var Tan = new MathFunctions.Tan { Position = Position };
                        CheckArguments(1);
                        Tan.Argument = Arguments[0].Symbol;
                        Symbol = Tan;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "atan")
                    {
                        var ATan = new MathFunctions.ATan { Position = Position };
                        CheckArguments(1);
                        ATan.Argument = Arguments[0].Symbol;
                        Symbol = ATan;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "ln")
                    {
                        var LN = new MathFunctions.LN { Position = Position };
                        CheckArguments(1);
                        LN.Argument = Arguments[0].Symbol;
                        Symbol = LN;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "log")
                    {
                        var Log = new MathFunctions.Log { Position = Position };
                        CheckArguments(1);
                        Log.Argument = Arguments[0].Symbol;
                        Symbol = Log;
                        Operator = OperatorType.No;
                    }
                    else if (LowerValue == "round")
                    {
                        var Round = new MathFunctions.Round { Position = Position };
                        CheckArguments(2);
                        Round.Left = Arguments[0].Symbol;
                        Round.Right = Arguments[1].Symbol;
                        Symbol = Round;
                        Operator = OperatorType.No;
                    }

                    //Ternary operator
                    else if (LowerValue == "iif")
                    {
                        var TernaryOperator = new Expressions.TernaryOperator { Position = Position };
                        CheckArguments(3);
                        TernaryOperator.Condition = Arguments[0].Symbol;
                        TernaryOperator.TruePart = Arguments[1].Symbol;
                        TernaryOperator.FalsePart = Arguments[2].Symbol;
                        Symbol = TernaryOperator;
                        Operator = OperatorType.No;
                    }

                    else
                        throw new ParseException("Invalid function: " + Value, Position);
                }
            }
            else if (Values.Length == 2)
            {
                //Element property
                switch (Values[1])
                {
                    case "Signed":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SIGNED) { Code = Values[0], Position = Position };
                        break;
                    case "SignedBy":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SIGNED_BY) { Code = Values[0], Position = Position };
                        break;
                    case "SignedOn":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SIGNED_ON) { Code = Values[0], Position = Position };
                        break;
                    case "HistoryLength":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.HISTORY_LENGTH) { Code = Values[0], Position = Position };
                        break;
                    case "Value":
                        Symbol = new ElementProperties.FieldProperty { Code = Values[0], Position = Position };
                        break;
                    case "ModifiedBy":
                        Symbol = new ElementProperties.EditableElementProperty(ElementProperties.EditableElementProperty.MODIFIED_BY) { Code = Values[0], Position = Position };
                        break;
                    case "ModifiedOn":
                        Symbol = new ElementProperties.EditableElementProperty(ElementProperties.EditableElementProperty.MODIFIED_ON) { Code = Values[0], Position = Position };
                        break;
                    case "Valid":
                        Symbol = new ElementProperties.ValidityElementProperty { Code = Values[0], Position = Position };
                        break;
                    case "Count":
                    case "IterationIndex":
                        Symbol = new ElementProperties.IteratorProperty(Values[1]) { Code = Values[0], Position = Position };
                        break;
                    case "Sum":
                    case "Avg":
                    case "StDev":
                        var Measure = new IteratorFunctions.Measure(Values[1]) { IteratorCode = Values[0], Position = Position };
                        ParseArgument(ref ExpressionValue, ref Position, 1);
                        Measure.Argument = Arguments[0].Symbol;
                        Symbol = Measure;
                        break;
                    case "Any":
                    case "All":
                        var Ispection = new IteratorFunctions.Ispection(Values[1]) { IteratorCode = Values[0], Position = Position };
                        ParseArgument(ref ExpressionValue, ref Position, 1);
                        Ispection.Argument = Arguments[0].Symbol;
                        Symbol = Ispection;
                        break;
                    case "Corr":
                    case "Slope":
                    case "Intercept":
                        var GeometricFunction = new IteratorFunctions.GeometricFunction(Values[1]) { IteratorCode = Values[0], Position = Position };
                        ParseArgument(ref ExpressionValue, ref Position, 2);
                        GeometricFunction.Left = Arguments[0].Symbol;
                        GeometricFunction.Right = Arguments[1].Symbol;
                        Symbol = GeometricFunction;
                        break;
                    case "Each":
                        var Each = new IteratorFunctions.Each { IteratorCode = Values[0], Position = Position };
                        ParseArgument(ref ExpressionValue, ref Position, 1);
                        Each.Argument = Arguments[0].Symbol;
                        Symbol = Each;
                        break;
                    case "IterationValue":
                        var IterationValue = new IteratorFunctions.IterationValue { IteratorCode = Values[0], Position = Position };
                        ParseArgument(ref ExpressionValue, ref Position, 2);
                        IterationValue.Left = Arguments[0].Symbol;
                        IterationValue.Right = Arguments[1].Symbol;
                        Symbol = IterationValue;
                        break;
                    case "RecurrenceTerm":
                        var RecurrenceTerm = new IteratorFunctions.RecurrenceTerm { IteratorCode = Values[0], Position = Position };
                        ParseArgument(ref ExpressionValue, ref Position, 2);
                        RecurrenceTerm.Left = Arguments[0].Symbol;
                        RecurrenceTerm.Right = Arguments[1].Symbol;
                        Symbol = RecurrenceTerm;
                        break;
                    default:
                        throw new ParseException("Invalid property: " + Value, Position);
                }
                Operator = OperatorType.No;
            }
            else
                throw new ParseException("Invalid identifier: " + Value, Position);
        }

        public List<TokenSet> Arguments { get; private set; }

        private bool ParseArgument(ref string ExpressionValue, ref int Position, int? ArgumentsCount = null)
        {
            int
                Caret = Position,
                ArgumentPosition,
                Depth = 1;

            if (Caret >= ExpressionValue.Length)
                return ArgumentsCount.HasValue ? throw new ParseException("( expected", Caret) : false;

            while (true)
            {
                var Character = ExpressionValue[Caret];
                ++Caret;

                if (Character == '(') break;

                if (!char.IsWhiteSpace(Character) || Caret == ExpressionValue.Length)
                    return ArgumentsCount.HasValue ? throw new ParseException("( expected", Caret) : false;
            }

            Arguments = new List<TokenSet>();
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

            if (Arguments.Count() > MAX_ARGUMENTS)
                throw new ParseException($"Too many arguments", Position);

            Position = Caret;

            if (ArgumentsCount.HasValue) CheckArguments(ArgumentsCount.Value);
            return true;
        }

        private void CheckArguments(int Count)
        {
            if (Arguments.Count() != Count)
                throw new ParseException($"The operator {Value} accepts exactly {Count} arguments", Position);
        }
    }
}