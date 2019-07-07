using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class TypedValue
    {
        public TypedValue(Type RawType, object Value) : this(DataType.FromType(RawType), Value.ToString()) { }

        public TypedValue(DataType Type, string SerializedValue)
        {
            _Type = Type;
            _SerializedValue = SerializedValue;
            Parse();
        }

        public TypedValue(DataType Type, object Value)
        {
            try
            {
                if (Value == null)
                {
                    _Type = Type;
                    this.Value = _SerializedValue = null;
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
                    _Type = Type;
                    this.Value = Value;
                    _SerializedValue = Value.ToString();
                }
            }
            catch (Exception Exception)
            {
                _Type = DataType.Error;
                this.Value = Exception.Message;
            }
        }

        private void Parse()
        {
            try
            {
                Value =
                    string.IsNullOrWhiteSpace(_SerializedValue) ? null :
                    Type == DataType.Boolean ? bool.Parse(_SerializedValue) : 
                    Type == DataType.DateTime ? DateTimeOffset.ParseExact(_SerializedValue, new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm" }, null, DateTimeStyles.AssumeUniversal).ToUniversalTime() :
                    Type == DataType.Date ? DateTimeOffset.ParseExact(_SerializedValue, new[] { "yyyy-MM-dd" }, null, DateTimeStyles.AssumeUniversal).ToUniversalTime() :
                    Type == DataType.Time ? TimeSpan.ParseExact(_SerializedValue, new[] { "hh\\:mm" }, null) :
                    Type == DataType.Number ? decimal.Parse(_SerializedValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture) :
                    Type == DataType.Array ? JsonConvert.DeserializeObject<IEnumerable<TypedValue>>(_SerializedValue) :
                    Type == DataType.Error ? _SerializedValue :
                    (object)_SerializedValue;
            }
            catch (Exception Exception)
            {
                _Type = DataType.Error;
                Value = _SerializedValue = Exception.Message;
            }
        }

        private DataType _Type;
        public DataType Type
        {
            get
            {
                return _Type;
            }
            private set
            {
                _Type = value;
                Parse();
            }
        }

        private string _SerializedValue;
        public String SerializedValue
        {
            get
            {
                return _SerializedValue;
            }

            set
            {
                _SerializedValue = value;
                Parse();
            }
        }

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
