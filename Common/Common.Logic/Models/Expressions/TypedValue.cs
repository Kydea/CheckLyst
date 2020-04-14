using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class TypedValue
    {
        private TypedValue()
        {

        }

        public TypedValue(DataType Type, string Value, string TimeZone)
        {
            try
            {
                this.Value =
                    string.IsNullOrWhiteSpace(Value) ? null :
                    Type == DataType.Boolean ? bool.Parse(Value) :
                    Type == DataType.DateTime ? DateTimeOffsetHelper.Parse(Value, TimeZone, out var Ambiguous, out var AmbiguousTimesSpans, $"{DateTimeOffsetHelper.DateFormat} {DateTimeOffsetHelper.TimeFormat}") :
                    Type == DataType.Date ? DateTimeOffsetHelper.Parse(Value, TimeZone, out Ambiguous, out AmbiguousTimesSpans, DateTimeOffsetHelper.DateFormat) :
                    Type == DataType.Time ? TimeSpan.ParseExact(Value, "hh\\:mm", null) :
                    Type == DataType.Number ? decimal.Parse(Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture) :
                    Type == DataType.Error ? Value :
                    (object)Value;
                this.Type = Type;
            }
            catch (Exception Exception)
            {
                this.Value = Exception.Message;
                this.Type = DataType.Error;
            }
        }

        public TypedValue(DataType Type, object Value)
        {
            try
            {
                if (Value == null)
                {
                    this.Type = Type;
                    this.Value = null;
                }
                else if
                (
                    Type == DataType.Boolean && !(Value is bool) ||
                    Type == DataType.DateTime && !(Value is DateTimeOffset) ||
                    Type == DataType.Date && !(Value is DateTimeOffset) ||
                    Type == DataType.Time && !(Value is TimeSpan) ||
                    Type == DataType.Number && !(Value is decimal) ||
                    Type == DataType.Array && !(Value is List<TypedValue>) ||
                    Type == DataType.Text && !(Value is string)
                )
                    throw new Exception("Invalid class " + Value.GetType().Name + " for DataType " + Type);
                else
                {
                    this.Type = Type;
                    this.Value = Value;
                }
            }
            catch (Exception Exception)
            {
                this.Type = DataType.Error;
                this.Value = Exception.Message;
            }
        }

        public DataType Type { get; set; }

        public object Value { get; set; }

        [JsonIgnore]
        public bool IsNull { get { return Value == null; } }

        [JsonIgnore]
        public bool BooleanValue { get { return Convert.ToBoolean(Value ?? false); } }

        [JsonIgnore]
        public decimal NumberValue { get { return Convert.ToDecimal(Value ?? 0m); } }

        [JsonIgnore]
        public DateTimeOffset DateTimeValue { get { return (DateTimeOffset)(Value ?? DateTimeOffset.MinValue); } }

        [JsonIgnore]
        public TimeSpan TimeSpanValue { get { return (TimeSpan)(Value ?? TimeSpan.Zero); } }

        [JsonIgnore]
        public string TextValue { get { return (string)Value; } }

        [JsonIgnore]
        public string ErrorValue { get { return (string)Value; } }

        [JsonIgnore]
        public List<TypedValue> ArrayValue { get { return (List<TypedValue>)Value; } }
    }
}
