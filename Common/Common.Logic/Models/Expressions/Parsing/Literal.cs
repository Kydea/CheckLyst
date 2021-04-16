using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Literal : Token
    {
        private const int MAX_ARGUMENTS = 6;

        public Literal(ref string ExpressionValue, Position Position, int Index) : base(
            ref ExpressionValue, Position, Index, (c, i) =>
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
                    Symbol = new Constant(new TypedValue(DataType.Boolean, true)) { Position = this.Position };
                    Operator = OperatorType.No;
                }
                else if (LowerValue == "false")
                {
                    Symbol = new Constant(new TypedValue(DataType.Boolean, false)) { Position = this.Position };
                    Operator = OperatorType.No;
                }

                //Logic operators
                else if (LowerValue == "or")
                    Symbol = new LogicOperators.Or { Position = this.Position };
                else if (LowerValue == "and")
                    Symbol = new LogicOperators.And { Position = this.Position };
                else if (LowerValue == "not")
                {
                    Symbol = new LogicOperators.Not { Position = this.Position };
                    TokenOrderRightToLeft = true;
                    Operator = OperatorType.Right;
                }

                //Expression value
                else if (!ParseArgument(ref ExpressionValue, Position))
                {
                    Symbol = new ExpressionValue { ExpressionCode = Value, Position = this.Position };
                    Operator = OperatorType.No;
                }

                //Functions
                else switch (LowerValue)
                    {
                        case "dateincrement":
                            var DateIncrement = new Functions.DateIncrement { Position = this.Position };
                            CheckArguments(4);
                            DateIncrement.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            Symbol = DateIncrement;
                            Operator = OperatorType.No;
                            break;
                        case "datetimeincrement":
                            var DateTimeIncrement = new Functions.DateTimeIncrement { Position = this.Position };
                            CheckArguments(6);
                            DateTimeIncrement.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            Symbol = DateTimeIncrement;
                            Operator = OperatorType.No;
                            break;
                        case "error":
                            var Error = new Functions.Error { Position = this.Position };
                            CheckArguments(1);
                            Error.Argument = Arguments[0].Symbol;
                            Symbol = Error;
                            Operator = OperatorType.No;
                            break;
                        case "null":
                            var Null = new Functions.Null { Position = this.Position };
                            CheckArguments(1);
                            Null.Argument = Arguments[0].Symbol;
                            Symbol = Null;
                            Operator = OperatorType.No;
                            break;
                        case "date":
                            var Date = new Functions.Date { Position = this.Position };
                            CheckArguments(3);
                            Date.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            Symbol = Date;
                            Operator = OperatorType.No;
                            break;
                        case "datetime":
                            var DateTime = new Functions.DateTime { Position = this.Position };
                            CheckArguments(5);
                            DateTime.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            Symbol = DateTime;
                            Operator = OperatorType.No;
                            break;
                        case "time":
                            var Time = new Functions.Time { Position = this.Position };
                            CheckArguments(2);
                            Time.Left = Arguments[0].Symbol;
                            Time.Right = Arguments[1].Symbol;
                            Symbol = Time;
                            Operator = OperatorType.No;
                            break;
                        case "statedatetime":
                            var StateDateTime = new Functions.StateDateTime { Position = this.Position };
                            CheckArguments(1);
                            StateDateTime.Argument = Arguments[0].Symbol;
                            Symbol = StateDateTime;
                            Operator = OperatorType.No;
                            break;

                        //Math functions
                        case "sqrt":

                            var SqRt = new MathFunctions.SqRt { Position = this.Position };
                            CheckArguments(1);
                            SqRt.Argument = Arguments[0].Symbol;
                            Symbol = SqRt;
                            Operator = OperatorType.No;
                            break;
                        case "sin":
                            var Sin = new MathFunctions.Sin { Position = this.Position };
                            CheckArguments(1);
                            Sin.Argument = Arguments[0].Symbol;
                            Symbol = Sin;
                            Operator = OperatorType.No;
                            break;
                        case "cos":
                            var Cos = new MathFunctions.Cos { Position = this.Position };
                            CheckArguments(1);
                            Cos.Argument = Arguments[0].Symbol;
                            Symbol = Cos;
                            Operator = OperatorType.No;
                            break;
                        case "tan":
                            var Tan = new MathFunctions.Tan { Position = this.Position };
                            CheckArguments(1);
                            Tan.Argument = Arguments[0].Symbol;
                            Symbol = Tan;
                            Operator = OperatorType.No;
                            break;
                        case "abs":
                            var Abs = new MathFunctions.Abs { Position = this.Position };
                            CheckArguments(1);
                            Abs.Argument = Arguments[0].Symbol;
                            Symbol = Abs;
                            Operator = OperatorType.No;
                            break;
                        case "atan":
                            var ATan = new MathFunctions.ATan { Position = this.Position };
                            CheckArguments(1);
                            ATan.Argument = Arguments[0].Symbol;
                            Symbol = ATan;
                            Operator = OperatorType.No;
                            break;
                        case "ln":
                            var LN = new MathFunctions.LN { Position = this.Position };
                            CheckArguments(1);
                            LN.Argument = Arguments[0].Symbol;
                            Symbol = LN;
                            Operator = OperatorType.No;
                            break;
                        case "log":
                            var Log = new MathFunctions.Log { Position = this.Position };
                            CheckArguments(1);
                            Log.Argument = Arguments[0].Symbol;
                            Symbol = Log;
                            Operator = OperatorType.No;
                            break;
                        case "round":
                            var Round = new MathFunctions.Round { Position = this.Position };
                            CheckArguments(2);
                            Round.Left = Arguments[0].Symbol;
                            Round.Right = Arguments[1].Symbol;
                            Symbol = Round;
                            Operator = OperatorType.No;
                            break;

                        //String functions
                        case "format":
                            var Format = new StringFunctions.Format { Position = this.Position };
                            CheckArguments(2);
                            Format.Left = Arguments[0].Symbol;
                            Format.Right = Arguments[1].Symbol;
                            Symbol = Format;
                            Operator = OperatorType.No;
                            break;
                        case "stringlength":
                            var Length = new StringFunctions.Length { Position = this.Position };
                            CheckArguments(1);
                            Length.Argument = Arguments[0].Symbol;
                            Symbol = Length;
                            Operator = OperatorType.No;
                            break;
                        case "stringtolowercase":
                        case "stringtouppercase":
                            var ToggleCase = new StringFunctions.ToggleCase(LowerValue) { Position = this.Position };
                            CheckArguments(1);
                            ToggleCase.Argument = Arguments[0].Symbol;
                            Symbol = ToggleCase;
                            Operator = OperatorType.No;
                            break;
                        case "stringfind":
                            var Find = new StringFunctions.Find { Position = this.Position };
                            CheckArguments(2, 3);
                            Find.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            if (Arguments.Count == 2) Find.Arguments.Add(new Constant(new TypedValue(DataType.Number, null)));
                            Symbol = Find;
                            Operator = OperatorType.No;
                            break;
                        case "stringreplace":
                            var Replace = new StringFunctions.Replace { Position = this.Position };
                            CheckArguments(3);
                            Replace.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            Symbol = Replace;
                            Operator = OperatorType.No;
                            break;
                        case "substring":
                            var Substring = new StringFunctions.Substring { Position = this.Position };
                            CheckArguments(2, 3);
                            Substring.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            if (Arguments.Count == 2) Substring.Arguments.Add(new Constant(new TypedValue(DataType.Number, null)));
                            Symbol = Substring;
                            Operator = OperatorType.No;
                            break;
                        case "stringleft":
                        case "stringright":
                            var Extreme = new StringFunctions.Extreme(LowerValue) { Position = this.Position };
                            CheckArguments(2);
                            Extreme.Left = Arguments[0].Symbol;
                            Extreme.Right = Arguments[1].Symbol;
                            Symbol = Extreme;
                            Operator = OperatorType.No;
                            break;
                        case "stringleftpad":
                        case "stringrightpad":
                            var Pad = new StringFunctions.Pad(LowerValue) { Position = this.Position };
                            CheckArguments(2, 3);
                            Pad.Arguments.AddRange(Arguments.Select(a => a.Symbol));
                            if (Arguments.Count == 2) Pad.Arguments.Add(new Constant(new TypedValue(DataType.Number, null)));
                            Symbol = Pad;
                            Operator = OperatorType.No;
                            break;
                        case "stringlefttrim":
                        case "stringrigthtrim":
                        case "stringtrim":
                            var Trim = new StringFunctions.Trim(LowerValue) { Position = this.Position };
                            CheckArguments(1, 2);
                            Trim.Left = Arguments[0].Symbol;
                            Trim.Right = Arguments.Count == 2 ? Arguments[1].Symbol : new Constant(new TypedValue(DataType.Text, " "));
                            Symbol = Trim;
                            Operator = OperatorType.No;
                            break;
                        case "stringtonumber":
                            var NumberValue = new StringFunctions.NumberValue { Position = this.Position };
                            CheckArguments(1);
                            NumberValue.Argument = Arguments[0].Symbol;
                            Symbol = NumberValue;
                            Operator = OperatorType.No;
                            break;

                        //Conditional operator
                        case "iif":
                            var ConditionalOperator = new Expressions.ConditionalOperator { Position = this.Position };
                            CheckArguments(3);
                            ConditionalOperator.Condition = Arguments[0].Symbol;
                            ConditionalOperator.TruePart = Arguments[1].Symbol;
                            ConditionalOperator.FalsePart = Arguments[2].Symbol;
                            Symbol = ConditionalOperator;
                            Operator = OperatorType.No;
                            break;
                        default:
                            throw new ParseException("Invalid function: " + Value, Position);
                    }
            }
            else if (Values.Length == 2)
            {
                //Element property
                switch (Values[1])
                {
                    case "Signed":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SIGNED) { Code = Values[0], Position = this.Position };
                        break;
                    case "SignedBy":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SIGNED_BY) { Code = Values[0], Position = this.Position };
                        break;
                    case "SignedOn":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.SIGNED_ON) { Code = Values[0], Position = this.Position };
                        break;
                    case "HistoryLength":
                        Symbol = new ElementProperties.SignatureProperty(ElementProperties.SignatureProperty.HISTORY_LENGTH) { Code = Values[0], Position = this.Position };
                        break;
                    case "Value":
                        Symbol = new ElementProperties.FieldProperty { Code = Values[0], Position = this.Position };
                        break;
                    case "ModifiedBy":
                        Symbol = new ElementProperties.EditableElementProperty(ElementProperties.EditableElementProperty.MODIFIED_BY) { Code = Values[0], Position = this.Position };
                        break;
                    case "ModifiedOn":
                        Symbol = new ElementProperties.EditableElementProperty(ElementProperties.EditableElementProperty.MODIFIED_ON) { Code = Values[0], Position = this.Position };
                        break;
                    case "Valid":
                        Symbol = new ElementProperties.ValidityElementProperty { Code = Values[0], Position = this.Position };
                        break;
                    case "Count":
                    case "IterationIndex":
                        Symbol = new ElementProperties.IteratorProperty(Values[1]) { Code = Values[0], Position = this.Position };
                        break;
                    case "Sum":
                    case "Avg":
                    case "StDev":
                        var Measure = new IteratorFunctions.Measure(Values[1]) { IteratorCode = Values[0], Position = this.Position };
                        ParseArgument(ref ExpressionValue, Position, 1);
                        Measure.Argument = Arguments[0].Symbol;
                        Symbol = Measure;
                        break;
                    case "Any":
                    case "All":
                        var Ispection = new IteratorFunctions.Ispection(Values[1]) { IteratorCode = Values[0], Position = this.Position };
                        ParseArgument(ref ExpressionValue, Position, 1);
                        Ispection.Argument = Arguments[0].Symbol;
                        Symbol = Ispection;
                        break;
                    case "Corr":
                    case "Slope":
                    case "Intercept":
                        var GeometricFunction = new IteratorFunctions.GeometricFunction(Values[1]) { IteratorCode = Values[0], Position = this.Position };
                        ParseArgument(ref ExpressionValue, Position, 2);
                        GeometricFunction.Left = Arguments[0].Symbol;
                        GeometricFunction.Right = Arguments[1].Symbol;
                        Symbol = GeometricFunction;
                        break;
                    case "Each":
                        var Each = new IteratorFunctions.Each { IteratorCode = Values[0], Position = this.Position };
                        ParseArgument(ref ExpressionValue, Position, 1);
                        Each.Argument = Arguments[0].Symbol;
                        Symbol = Each;
                        break;
                    case "IterationValue":
                        var IterationValue = new IteratorFunctions.IterationValue { IteratorCode = Values[0], Position = this.Position };
                        ParseArgument(ref ExpressionValue, Position, 2);
                        IterationValue.Left = Arguments[0].Symbol;
                        IterationValue.Right = Arguments[1].Symbol;
                        Symbol = IterationValue;
                        break;
                    case "RecurrenceTerm":
                        var RecurrenceTerm = new IteratorFunctions.RecurrenceTerm { IteratorCode = Values[0], Position = new Position(Position) };
                        ParseArgument(ref ExpressionValue, Position, 2);
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

        private bool ParseArgument(ref string ExpressionValue, Position Position, int? ArgumentsCount = null)
        {
            int Depth = 1;

            if (Position.Index >= ExpressionValue.Length)
                return ArgumentsCount.HasValue ? throw new ParseException("( expected", Position) : false;

            while (true)
            {
                var Character = ExpressionValue[Position.Index];

                if (Character == '(')
                {
                    Position.Increment(false);
                    break;
                }

                if (!char.IsWhiteSpace(Character) || Position.Index + 1 == ExpressionValue.Length)
                    return ArgumentsCount.HasValue ? throw new ParseException("( expected", Position) : false;

                Position.Increment(Character == '\n');
            }

            Arguments = new List<TokenSet>();
            var Caret = Position.Index;
            bool InQuote = false;

            while (Caret < ExpressionValue.Length && Depth > 0)
            {
                var Character = ExpressionValue[Caret];
                if (!InQuote && Character == '(')
                    ++Depth;
                else if (!InQuote && Character == ')')
                    --Depth;
                else if (Character == '"')
                    InQuote = !InQuote; 

                if (!InQuote && Depth == 1 && Character == ',' || Depth == 0)
                {
                    Arguments.Add(new TokenSet(ExpressionValue, Position, Caret));
                    Position.Increment(false);
                }

                ++Caret;
            }

            if (Depth > 0) throw new ParseException(") expected", Position);

            if (Arguments.Count() > MAX_ARGUMENTS)
                throw new ParseException($"Too many arguments", Position);

            if (ArgumentsCount.HasValue) CheckArguments(ArgumentsCount.Value);
            return true;
        }

        private void CheckArguments(int Count)
        {
            if (Arguments.Count() != Count)
                throw new ParseException($"The operator {Value} accepts exactly {Count} arguments", Position);
        }

        private void CheckArguments(int Min, int Max)
        {
            if (Arguments.Count() < Min)
                throw new ParseException($"The operator {Value} accepts at least {Min} arguments", Position);
            if (Arguments.Count() > Max)
                throw new ParseException($"The operator {Value} accepts maximum {Max} arguments", Position);
        }
    }
}