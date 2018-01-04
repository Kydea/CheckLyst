using System;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class TypedValue
    {
        public TypedValue(Type RawType, Object Value) : this(DataType.FromType(RawType), Value.ToString()) { }

        public TypedValue(DataType Type, String SerializedValue)
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
                    Type == DataType.Boolean && !(Value is Boolean) ||
                    Type == DataType.DateTime && !(Value is DateTimeOffset) ||
                    Type == DataType.Date && !(Value is DateTimeOffset) ||
                    Type == DataType.Time && !(Value is TimeSpan) ||
                    Type == DataType.Number && !(Value is Decimal) ||
                    Type == DataType.Text && !(Value is String)
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
                    String.IsNullOrWhiteSpace(_SerializedValue) ? null :
                    Type == DataType.Boolean ? "true".Equals(_SerializedValue, StringComparison.InvariantCultureIgnoreCase) :
                    Type == DataType.DateTime ? DateTimeOffset.ParseExact(_SerializedValue, new[] { "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm" }, null, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime() :
                    Type == DataType.Date ? DateTimeOffset.ParseExact(_SerializedValue, new[] { "dd/MM/yyyy" }, null, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime() :
                    Type == DataType.Time ? TimeSpan.ParseExact(_SerializedValue, new[] { "hh\\:mm" }, null) :
                    Type == DataType.Number ? Decimal.Parse(_SerializedValue) :
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

        private String _SerializedValue;
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

        public bool IsNull { get { return Value == null; } }

        public bool BooleanValue { get { return (bool)(Value ?? false); } }

        public Decimal NumberValue { get { return (Decimal)(Value ?? 0); } }

        public DateTimeOffset DateTimeValue { get { return (DateTimeOffset)(Value ?? DateTimeOffset.MinValue); } }

        public TimeSpan TimeSpanValue { get { return (TimeSpan)(Value ?? 0); } }

        public String TextValue { get { return (String)Value; } }

        public String ErrorValue { get { return (String)Value; } }
    }
}
