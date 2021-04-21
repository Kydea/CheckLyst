using System;
using System.Globalization;

namespace CheckListManager.Common.Core.Helpers
{
    public static class DecimalHelper
    {
        public static decimal? Parse(string Value) =>
            Value == null ? default(decimal?) : decimal.TryParse(Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalResult) ? decimalResult : default(decimal?);

        public static void Parse(string Value, int? Precision, bool ScientificNotation, string DecimalSeparator, out decimal? Result, out string FormattedResult)
        {
            Result = Parse(Value);
            try
            {
                if (!Result.HasValue)
                    FormattedResult = null;
                else if (Result == 0m)
                    FormattedResult = "0";
                else if (ScientificNotation)
                {
                    var Exponent = (int)Math.Floor(Math.Log10((double)Result));
                    var Coefficent = (double)Result / Math.Pow(10, Exponent);
                    if (Precision.HasValue) Coefficent = Math.Round(Coefficent, Precision.Value);
                    Result = (decimal)(Coefficent * Math.Pow(10, Exponent));
                    FormattedResult = Coefficent + "e" + (Exponent >= 0 ? "+" : string.Empty) + Exponent;
                }
                else
                {
                    Result = Precision.HasValue ? Math.Round(Result.Value, Precision.Value) : Result;
                    FormattedResult = Result.ToString().Replace(",", DecimalSeparator).Replace(".", DecimalSeparator);
                }
            }
            catch
            {
                FormattedResult = null;
            }
        }
    }
}
