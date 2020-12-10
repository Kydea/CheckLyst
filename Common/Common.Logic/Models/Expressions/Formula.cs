using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
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

        [JsonProperty]
        private string Format { get; set; }
        public bool IsSet { get; set; }
        public List<string> ValueKeys { get; } = new List<string>();
        [JsonProperty]
        private List<string> TokenFormats { get; } = new List<string>();
        public string Value { get; set; }
        public bool HasValue { get; set; } //has a value and does not contains null or error values

        public void BuildDependencies(string RawValue, Formula BaseFormula)
        {
            if (BaseFormula == null)
            {
                ValueKeys.Clear();
                TokenFormats.Clear();
                Value = null;
                Format =
                    string.IsNullOrWhiteSpace(RawValue) ?
                        null :
                        new Regex(@"(?<Start>\{)+(?<Property>[\w\-\.\[\]]+)(?<Format>:[^}]+)?(?<End>\})+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
                            .Replace(RawValue, delegate (Match Match)
                            {
                                var Key = Match.Groups["Property"].Value;
                                ValueKeys.Add(Key);
                                string Format = Match.Groups["Format"].Value;
                                if (!string.IsNullOrWhiteSpace(Format))
                                {
                                    Format = Format.Substring(1);
                                    if (Format.StartsWith("n") && !int.TryParse(Format.Substring(1), out int Number))
                                        Format = $"0.{new string('0', Number)}";
                                }
                                TokenFormats.Add(Format);
                                return new string('{', Match.Groups["Start"].Captures.Count) + (ValueKeys.Count - 1) + new string('}', Match.Groups["End"].Captures.Count);
                            });
                IsSet = Format != null;
            }
            else
                IsSet = BaseFormula.IsSet;
        }

        private bool Evaluated { get; set; }
        private void Evaluate(ICheckList CheckList, Dictionary<string, object> RuntimeValues, string DecimalSeparator, string TimeZone, List<int> IterationIndexes, string Format, List<string> ValueKeys, List<string> TokenFormats)
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
                            ExpressionValue.Type == DataType.DateTime ? ExpressionValue.DateTimeValue.ToLocalTime(TimeZone).ToString(TokenFormats[Index] ?? "yyyy-MM-dd HH:mm:ss") :
                            ExpressionValue.Type == DataType.Date ? ExpressionValue.DateTimeValue.ToString(TokenFormats[Index] ?? "yyyy-MM-dd") :
                            ExpressionValue.Type == DataType.Time ? ExpressionValue.TimeSpanValue.ToString(TokenFormats[Index] ?? @"hh\:mm\:ss") :
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

        public void Evaluate(ICheckList CheckList, Dictionary<string, object> RuntimeValues, string DecimalSeparator, string TimeZone, List<int> IterationIndexes, Formula BaseFormula)
        {
            if (Evaluated) return;
            Evaluated = true;
            try
            {
                Evaluate(CheckList, RuntimeValues, DecimalSeparator, TimeZone, IterationIndexes, BaseFormula.Format, BaseFormula.ValueKeys, BaseFormula.TokenFormats);
            }
            catch
            {
                Value = $"{ErrorValueLeft}Invalid formula{ErrorValueRight}";
                HasValue = false;
            }
        }

        public void Evaluate(ICheckList CheckList, Dictionary<string, object> RuntimeValues, string DecimalSeparator, string TimeZone, List<int> IterationIndexes)
        {
            if (Evaluated) return;
            Evaluated = true;
            try
            {
                Evaluate(CheckList, RuntimeValues, DecimalSeparator, TimeZone, IterationIndexes, Format, ValueKeys, TokenFormats);
            }
            catch
            {
                Value = $"{ErrorValueLeft}Invalid formula{ErrorValueRight}";
                HasValue = false;
            }
        }
    }
}
