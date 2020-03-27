using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Formula
    {
        public const string
            NullValue = "<span class='Null'></span>",
            ErrorValueLeft = "<span class='Error'>",
            ErrorValueRight = "</span>";

        public string Format { get; set; }
        public bool IsSet => Format != null;
        public List<string> ValueKeys { get; } = new List<string>();
        public List<string> TokenFormats { get; } = new List<string>();
        public string Value { get; set; }
        public bool HasValue { get; set; } //has a value and ha no nulls or errors

        public void BuildDependencies(string RawValue)
        {
            Format = 
                string.IsNullOrWhiteSpace(RawValue) ?
                    null :
                    new Regex(@"(?<Start>\{)+(?<Property>[\w\-\.\[\]]+)(?<Format>:[^}]+)?(?<End>\})+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
                        .Replace(RawValue, delegate (Match Match)
                        {
                            ValueKeys.Add(Match.Groups["Property"].Value);
                            string Format = Match.Groups["Format"].Value;
                            if (!string.IsNullOrWhiteSpace(Format))
                            {
                                if (!int.TryParse(Format.Substring(2), out int Number)) Number = -1;
                                Format =
                                    Number < 0 ? string.Empty :
                                    Format.StartsWith(":n") ? "0." + new string('0', Number) :
                                    string.Empty;
                            }
                            TokenFormats.Add(Format);
                            return new string('{', Match.Groups["Start"].Captures.Count) + (ValueKeys.Count - 1) + new string('}', Match.Groups["End"].Captures.Count);
                        });
        }

        public void Evaluate(ICheckList CheckList, Dictionary<string, object> RuntimeValues, string DecimalSeparator, string TimeZone, List<int> IterationIndexes)
        {
            try
            {
                int Index = 0;
                Value = Format == null ? null : string.Format(
                    CultureInfo.InvariantCulture,
                    Format,
                    ValueKeys.Select(Key =>
                    {
                        var ExpressionValue = CheckList.GetExpressionValue(Key, IterationIndexes, out var Error);
                        var Value =
                            ExpressionValue != null ?
                                ExpressionValue.Type == DataType.Error ? ErrorValueLeft + ExpressionValue.ErrorValue + ErrorValueRight :
                                ExpressionValue.IsNull ? NullValue :
                                ExpressionValue.Type == DataType.Number ? ExpressionValue.NumberValue.ToString(TokenFormats[Index]).Replace(".", DecimalSeparator).Replace(",", DecimalSeparator) :
                                ExpressionValue.Type == DataType.DateTime ? ExpressionValue.DateTimeValue.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm:ss") :
                                ExpressionValue.Type == DataType.Date ? ExpressionValue.DateTimeValue.ToString("yyyy-MM-dd") :
                                ExpressionValue.Type == DataType.Time ? ExpressionValue.TimeSpanValue.ToString(@"hh\:mm\:ss") :
                                ExpressionValue.Value :
                            RuntimeValues?.ContainsKey(Key) ?? false ? RuntimeValues[Key] :
                            $"{ErrorValueLeft}{Error}{ErrorValueRight}";
                        ++Index;
                        return Value;
                    }).ToArray());

                HasValue =
                    !string.IsNullOrEmpty(Format)
                    && !ValueKeys.Any(Key =>
                    {
                        var ExpressionValue = CheckList.GetExpressionValue(Key, IterationIndexes);
                        var Value = 
                            CheckList.Expressions.ContainsKey(Key) && (ExpressionValue == null || ExpressionValue.Type == DataType.Error || ExpressionValue.IsNull)
                            || !CheckList.Expressions.ContainsKey(Key) && !RuntimeValues.ContainsKey(Key);
                        return Value;
                    });
            }
            catch
            {
                Value = $"{ErrorValueLeft}Invalid formula{ErrorValueRight}";
                HasValue = false;
            }
        }
    }
}
