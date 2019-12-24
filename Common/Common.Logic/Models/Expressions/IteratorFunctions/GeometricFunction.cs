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
            var RightAvg = LeftValues.Average(TypedValue => TypedValue.NumberValue);

            decimal Result = 0;
            for(var Index = 0; Index < LeftValues.Count; ++Index)
                Result += (LeftValues[0].NumberValue - LeftAvg) * (RightValues[0].NumberValue - RightAvg);

            return Result / (decimal)Math.Sqrt(
                LeftValues.Sum(TypedValue => Math.Pow((double)(TypedValue.NumberValue - LeftAvg), 2))
                * RightValues.Sum(TypedValue => Math.Pow((double)(TypedValue.NumberValue - RightAvg), 2))
            );
        }

        private decimal Slope(List<TypedValue> LeftValues, List<TypedValue> RightValues, decimal? RightAvg)
        {
            var LeftAvg = LeftValues.Average(TypedValue => TypedValue.NumberValue);
            var CalculaterRightAvg = RightAvg ?? LeftValues.Average(TypedValue => TypedValue.NumberValue);

            decimal Result = 0;
            for (var Index = 0; Index < LeftValues.Count; ++Index)
                Result += (LeftValues[0].NumberValue - LeftAvg) * (RightValues[0].NumberValue - CalculaterRightAvg);

            return Result / (decimal)LeftValues.Sum(TypedValue => Math.Pow((double)(TypedValue.NumberValue - LeftAvg), 2));
        }

        private decimal Intercept(List<TypedValue> LeftValues, List<TypedValue> RightValues)
        {
            var RightAvg = LeftValues.Average(TypedValue => TypedValue.NumberValue);

            return RightAvg - Slope(LeftValues, RightValues, RightAvg);
        }

        protected override TypedValue GetValue(List<TypedValue> LeftValues, List<TypedValue> RightValues)
        {
            var Result =
                LeftValues.Any(Value => Value.IsNull) || RightValues.Any(Value => Value.IsNull) ? default(decimal?) :
                Function == "Corr" ? Corr(LeftValues, RightValues) :
                Function == "Slope" ? Slope(LeftValues, RightValues, null) :
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
