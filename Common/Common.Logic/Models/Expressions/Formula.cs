using CheckListManager.Common.Core.Helpers;
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
                                int Number;
                                if (!int.TryParse(Format.Substring(2), out Number)) Number = -1;
                                Format =
                                    Number < 0 ? string.Empty :
                                    Format.StartsWith(":n") ? "0." + new string('0', Number) :
                                    string.Empty;
                            }
                            Formats.Add(Format);
                            return new string('{', Match.Groups["Start"].Captures.Count) + (ValueKeys.Count - 1) + new string('}', Match.Groups["End"].Captures.Count);
                        });
        }

        public void Update(Dictionary<string, Expression> Expressions, Dictionary<string, object> RuntimeValues, string DecimalSeparator, string TimeZone)
        {
            try
            {
                int Index = 0;
                Value = Format == null ? null : string.Format(
                    CultureInfo.InvariantCulture,
                    Format,
                    ValueKeys.Select(Key =>
                    {
                        var Value =
                            Expressions.ContainsKey(Key) ?
                                Expressions[Key].Value.Type == DataType.Error ? ErrorValueLeft + Expressions[Key].Value.ErrorValue + ErrorValueRight :
                                Expressions[Key].Value.IsNull ? NullValue :
                                Expressions[Key].Value.Type == DataType.Number ? Expressions[Key].Value.NumberValue.ToString(Formats[Index]).Replace(".", DecimalSeparator).Replace(",", DecimalSeparator) :
                                Expressions[Key].Value.Type == DataType.DateTime ? Expressions[Key].Value.DateTimeValue.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm:ss") :
                                Expressions[Key].Value.Type == DataType.Date ? Expressions[Key].Value.DateTimeValue.ToString("yyyy-MM-dd") :
                                Expressions[Key].Value.Type == DataType.Time ? Expressions[Key].Value.TimeSpanValue.ToString(@"hh\:mm\:ss") :
                                Expressions[Key].Value.Value :
                            RuntimeValues?.ContainsKey(Key) ?? false ? RuntimeValues[Key] :
                            $"{ErrorValueLeft}Code not found: {Key}{ErrorValueRight}";
                        ++Index;
                        return Value;
                    }).ToArray());

                HasValue =
                    !string.IsNullOrEmpty(Format)
                    && !ValueKeys.Any(Key =>
                        Expressions.ContainsKey(Key) && (Expressions[Key].Value.Type == DataType.Error || Expressions[Key].Value.IsNull)
                        || !Expressions.ContainsKey(Key) && !RuntimeValues.ContainsKey(Key)
                    );
            }
            catch
            {
                Value = $"{ErrorValueLeft}Invalid formula{ErrorValueRight}";
                HasValue = false;
            }

        }

    }
}
