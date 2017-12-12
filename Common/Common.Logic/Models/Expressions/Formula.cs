using System;
using System.Collections.Generic;
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
        private List<String> ValueKeys = new List<string>();

		public Formula() { }

		public Formula(string RawValue = null)
        {
            this.RawValue = RawValue;
			Format = FormatValue(RawValue);
        }

		private String _RawValue;
        public String RawValue
		{
			get { return _RawValue; }
			set { _RawValue = value; Format = FormatValue(value); }
		}

        public String Value { get; private set; }

        public bool HasValue { get; private set; }

		private String FormatValue(String RawValue)
		{
			return
				String.IsNullOrWhiteSpace(RawValue) ?
					null :
					new Regex(@"(?<Start>\{)+(?<Property>[\w\-\.\[\]]+)(?<Format>:[^}]+)?(?<End>\})+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
						.Replace(RawValue, delegate (Match Match)
						{
							ValueKeys.Add(Match.Groups["Property"].Value);
                            String Format = Match.Groups["Format"].Value;
                            if (!String.IsNullOrWhiteSpace(Format))
                            {
                                int Number;
                                if (!int.TryParse(Format.Substring(2), out Number)) Number = -1;
                                Format =
                                    Number < 0 ? String.Empty :
                                    Format.StartsWith(":s") ? ":0." + new String('0', Number) :
                                    Format.StartsWith(":n") ? ":#,##0." + new String('0', Number) :
                                    String.Empty;
                            }
                            return new string('{', Match.Groups["Start"].Captures.Count) + (ValueKeys.Count - 1) + Format + new string('}', Match.Groups["End"].Captures.Count);
						});
		}

		public void Update(Dictionary<String, Expression> Expressions, Dictionary<String, Object> RuntimeValues)
        {
            Value = Format == null ? null : string.Format(
                Format, 
                ValueKeys.Select(Key =>
                    Expressions.ContainsKey(Key) ?
                        Expressions[Key].Value.Type == DataType.Error ? ErrorValueLeft + Expressions[Key].Value.ErrorValue + ErrorValueRight :
                        Expressions[Key].Value.IsNull ? NullValue :
                        Expressions[Key].Value.Type == DataType.DateTime ? Expressions[Key].Value.DateTimeValue.ToString("dd/MM/yyyy HH:mm:ss") :
                        Expressions[Key].Value.Type == DataType.Date ? Expressions[Key].Value.DateTimeValue.ToString("dd/MM/yyyy") :
                        Expressions[Key].Value.Type == DataType.Time ? Expressions[Key].Value.TimeSpanValue.ToString(@"hh\:mm\:ss") :
                        Expressions[Key].Value.Value :
                    RuntimeValues?.ContainsKey(Key) ?? false ? RuntimeValues[Key] :
                    ErrorValueLeft + "Code not found: " + Key + ErrorValueRight
               ).ToArray());

            HasValue = 
                !String.IsNullOrEmpty(Format)
                && !ValueKeys.Any(Key =>
                    Expressions.ContainsKey(Key) && (Expressions[Key].Value.Type == DataType.Error || Expressions[Key].Value.IsNull)
                    || !Expressions.ContainsKey(Key) && !RuntimeValues.ContainsKey(Key)
                );
        }

    }
}
