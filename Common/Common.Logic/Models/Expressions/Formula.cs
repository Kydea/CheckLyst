using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Formula
    {
        private string _format;
        private readonly List<string> _valueKeys = new List<string>();

		public Formula() { }

		public Formula(string rawValue = null)
        {
            RawValue = rawValue;
			_format = FormatValue(rawValue);
        }

		private string _rawValue;
        public string RawValue
		{
			get => _rawValue;
	        set { _rawValue = value; _format = FormatValue(value); }
		}

        public string Value { get; private set; }

        public bool HasValue { get; private set; }

		private string FormatValue(string rawValue)
		{
			return
				string.IsNullOrWhiteSpace(rawValue) ?
					null :
					new Regex(@"(?<Start>\{)+(?<Property>[\w\-\.\[\]]+)(?<Format>:[^}]+)?(?<End>\})+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
						.Replace(rawValue, delegate (Match match)
						{
							_valueKeys.Add(match.Groups["Property"].Value);
                            var format = match.Groups["Format"].Value;
                            if (!string.IsNullOrWhiteSpace(format))
                            {
	                            if (!int.TryParse(format.Substring(2), out var number)) number = -1;
                                format =
                                    number < 0 ? string.Empty :
                                    format.StartsWith(":s") ? ":0." + new string('0', number) :
                                    format.StartsWith(":n") ? ":#,##0." + new string('0', number) :
                                    string.Empty;
                            }
                            return new string('{', match.Groups["Start"].Captures.Count) + (_valueKeys.Count - 1) + format + new string('}', match.Groups["End"].Captures.Count);
						});
		}

		public void Update(Dictionary<string, Expression> expressions, Dictionary<string, object> runtimeValues)
        {
            Value = _format == null ? null : string.Format(
                _format, 
                _valueKeys.Select(key =>
                    expressions.ContainsKey(key) ?
                        expressions[key].Value.Type == DataType.Error ? "<span class='Error'>" + expressions[key].Value.ErrorValue + "</span>" :
                        expressions[key].Value.IsNull ? "<span class='Null'></span>" :
                        expressions[key].Value.Value :
                    runtimeValues?.ContainsKey(key) ?? false ? runtimeValues[key] :
                    "<span class='Error'>Code not found: " + key + "</span>"
               ).ToArray());

            HasValue = 
                !string.IsNullOrEmpty(_format)
                && !_valueKeys.Any(key =>
                    expressions.ContainsKey(key) && (expressions[key].Value.Type == DataType.Error || expressions[key].Value.IsNull)
                    || !expressions.ContainsKey(key) && !runtimeValues.ContainsKey(key)
                );
        }

    }
}
