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

        private string Format;
        private List<string> ValueKeys = new List<string>();
        private List<string> Formats = new List<string>();

        public Formula() { }

        public Formula(string RawValue)
        {
            this.RawValue = RawValue;
        }

        private string _RawValue;
        public string RawValue
        {
            get { return _RawValue; }
            set { _RawValue = value; Format = FormatValue(value); }
        }

        public string Value { get; private set; }

        public bool HasValue { get; private set; }

        private string FormatValue(string RawValue)
        {
            return
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
                            Formats.Add(Format);
                            return new string('{', Match.Groups["Start"].Captures.Count) + (ValueKeys.Count - 1) + new string('}', Match.Groups["End"].Captures.Count);
                        });
        }

        public void Update(ICheckList CheckList, Dictionary<string, object> RuntimeValues, string DecimalSeparator, string TimeZone, List<int> IterationIndexes)
        {
            try
            {
                int Index = 0;
                Value = Format == null ? null : string.Format(
                    CultureInfo.InvariantCulture,
                    Format,
                    ValueKeys.Select(Key =>
                    {
                        var ExpressionValue = CheckList.GetExpressionValue(Key, IterationIndexes);
                        var Value =
                            ExpressionValue != null ?
                                ExpressionValue.Type == DataType.Error ? ErrorValueLeft + ExpressionValue.ErrorValue + ErrorValueRight :
                                ExpressionValue.IsNull ? NullValue :
                                ExpressionValue.Type == DataType.Number ? ExpressionValue.NumberValue.ToString(Formats[Index]).Replace(".", DecimalSeparator).Replace(",", DecimalSeparator) :
                                ExpressionValue.Type == DataType.DateTime ? ExpressionValue.DateTimeValue.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm:ss") :
                                ExpressionValue.Type == DataType.Date ? ExpressionValue.DateTimeValue.ToString("yyyy-MM-dd") :
                                ExpressionValue.Type == DataType.Time ? ExpressionValue.TimeSpanValue.ToString(@"hh\:mm\:ss") :
                                ExpressionValue.Value :
                            RuntimeValues?.ContainsKey(Key) ?? false ? RuntimeValues[Key] :
                            $"{ErrorValueLeft}Code not found: {Key}{ErrorValueRight}";
                        ++Index;
                        return Value;
                    }).ToArray());

                HasValue =
                    !string.IsNullOrEmpty(Format)
                    && !ValueKeys.Any(Key =>
                    {
                        var ExpressionValue = CheckList.GetExpressionValue(Key, IterationIndexes);
                        var Value = 
                            CheckList.Expressions.ContainsKey(Key) && (ExpressionValue.Type == DataType.Error || ExpressionValue.IsNull)
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
