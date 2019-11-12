using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class TypedValue
    {
        public TypedValue(Type RawType, object Value) : this(DataType.FromType(RawType), Value.ToString()) { }

        public TypedValue(DataType Type, string SerializedValue, string TimeZone)
        {
            this.Type = Type;
            this.SerializedValue = SerializedValue;
            Parse(TimeZone);
        }

        public TypedValue(DataType Type, object Value)
        {
            try
            {
                if (Value == null)
                {
                    this.Type = Type;
                    this.Value = SerializedValue = null;
                }
                else if
                (
                    Type == DataType.Boolean && !(Value is bool) ||
                    Type == DataType.DateTime && !(Value is DateTimeOffset) ||
                    Type == DataType.Date && !(Value is DateTimeOffset) ||
                    Type == DataType.Time && !(Value is TimeSpan) ||
                    Type == DataType.Number && !(Value is decimal) ||
                    Type == DataType.Array && !(Value is IEnumerable<TypedValue>) ||
                    Type == DataType.Text && !(Value is string)
                )
                    throw new Exception("Invalid class " + Value.GetType().Name + " for DataType " + Type.ToString());
                else
                {
                    this.Type = Type;
                    this.Value = Value;
                    SerializedValue = Value.ToString();
                }
            }
            catch (Exception Exception)
            {
                this.Type = DataType.Error;
                this.Value = Exception.Message;
            }
        }

        private void Parse(string TimeZone)
        {
            try
            {
                Value =
                    string.IsNullOrWhiteSpace(SerializedValue) ? null :
                    Type == DataType.Boolean ? bool.Parse(SerializedValue) : 
                    Type == DataType.DateTime ? DateTimeOffsetHelper.Parse(SerializedValue, TimeZone, out var Ambiguous, out var AmbiguousTimesSpans, $"{DateTimeOffsetHelper.DateFormat} {DateTimeOffsetHelper.TimeFormat}") :
                    Type == DataType.Date ? DateTimeOffsetHelper.Parse(SerializedValue, TimeZone, out Ambiguous, out AmbiguousTimesSpans, DateTimeOffsetHelper.DateFormat) :
                    Type == DataType.Time ? TimeSpan.ParseExact(SerializedValue, "hh\\:mm", null) :
                    Type == DataType.Number ? decimal.Parse(SerializedValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture) :
                    Type == DataType.Array ? JsonConvert.DeserializeObject<IEnumerable<TypedValue>>(SerializedValue) :
                    Type == DataType.Error ? SerializedValue :
                    (object)SerializedValue;
            }
            catch (Exception Exception)
            {
                Type = DataType.Error;
                Value = SerializedValue = Exception.Message;
            }
        }

        public DataType Type { get; set; }

        public string SerializedValue { get; private set; }

        public object Value
        {
            get; private set;
        }

        [JsonIgnore]
        public bool IsNull { get { return Value == null; } }

        [JsonIgnore]
        public bool BooleanValue { get { return (bool)(Value ?? false); } }

        [JsonIgnore]
        public decimal NumberValue { get { return (decimal)(Value ?? 0m); } }

        [JsonIgnore]
        public DateTimeOffset DateTimeValue { get { return (DateTimeOffset)(Value ?? DateTimeOffset.MinValue); } }

        [JsonIgnore]
        public TimeSpan TimeSpanValue { get { return (TimeSpan)(Value ?? TimeSpan.Zero); } }

        [JsonIgnore]
        public string TextValue { get { return (string)Value; } }

        [JsonIgnore]
        public string ErrorValue { get { return (string)Value; } }

        [JsonIgnore]
        public IEnumerable<TypedValue> ArrayValue { get { return (IEnumerable<TypedValue>)Value; } }
    }
}
