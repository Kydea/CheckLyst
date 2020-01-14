using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class GeometricFunction : BinaryIteratorFunction
    {
        private readonly string[] Functions = { "Corr", "Slope", "Intercept" };

        public GeometricFunction(string Function)
        {
            if (!Function.In(Function)) throw new ArgumentException($"Invalid operation {Function}");
            this.Function = Function;
        }

        private string Function { get; }

        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            base.Check(Prototype, IteratorStack);
            if (DataType != DataType.Number)
                throw new ParseException($"Operands of function {Function} must be numbers", Position);
        }

        private decimal Corr(List<TypedValue> LeftValues, List<TypedValue> RightValues)
        {
            var LeftAvg = LeftValues.Average(TypedValue => TypedValue.NumberValue);
            var RightAvg = RightValues.Average(TypedValue => TypedValue.NumberValue);

            decimal Result = 0;
            for(var Index = 0; Index < LeftValues.Count; ++Index)
                Result += (LeftValues[Index].NumberValue - LeftAvg) * (RightValues[Index].NumberValue - RightAvg);

            decimal Denominator = (decimal)Math.Sqrt(
               LeftValues.Sum(TypedValue => Math.Pow((double)(TypedValue.NumberValue - LeftAvg), 2))
               * RightValues.Sum(TypedValue => Math.Pow((double)(TypedValue.NumberValue - RightAvg), 2))
            );

            return Denominator == 0 ? 0 : Result / Denominator;
        }

        private decimal Slope(List<TypedValue> LeftValues, List<TypedValue> RightValues, decimal? PreCalculatedLeftAvg, decimal? PreCalculatedRightAvg)
        {
            var LeftAvg = PreCalculatedLeftAvg ?? LeftValues.Average(TypedValue => TypedValue.NumberValue);
            var RightAvg = PreCalculatedRightAvg ?? RightValues.Average(TypedValue => TypedValue.NumberValue);

            decimal Result = 0;
            for (var Index = 0; Index < LeftValues.Count; ++Index)
                Result += (LeftValues[Index].NumberValue - LeftAvg) * (RightValues[Index].NumberValue - RightAvg);

            decimal Denominator = (decimal)LeftValues.Sum(TypedValue => Math.Pow((double)(TypedValue.NumberValue - LeftAvg), 2));
            return Denominator == 0 ? 0 : Result / Denominator;
        }

        private decimal Intercept(List<TypedValue> LeftValues, List<TypedValue> RightValues)
        {
            decimal
                LeftAvg = LeftValues.Average(TypedValue => TypedValue.NumberValue),
                RightAvg = RightValues.Average(TypedValue => TypedValue.NumberValue);

            return RightAvg - LeftAvg * Slope(LeftValues, RightValues, LeftAvg, RightAvg);
        }

        protected override TypedValue GetValue(List<TypedValue> LeftValues, List<TypedValue> RightValues)
        {
            var Result =
                LeftValues.Count == 0 || LeftValues.Any(Value => Value.IsNull) || RightValues.Any(Value => Value.IsNull) ? default(decimal?) :
                Function == "Corr" ? Corr(LeftValues, RightValues) :
                Function == "Slope" ? Slope(LeftValues, RightValues, null, null) :
                Function == "Intercept" ? Intercept(LeftValues, RightValues) :
                default(decimal?);

            return new TypedValue(DataType.Number, Result);
        }

        protected override string OperatorName => Function;

        public override string ToString()
        {
            return $"{OperatorName} ({Left?.ToString() ?? "#"}, {Right?.ToString() ?? "#"})";
        }
    }
}
