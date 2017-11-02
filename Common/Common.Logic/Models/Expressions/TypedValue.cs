using System;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class TypedValue
    {
        public TypedValue(Type rawType, object value) : this(DataType.FromType(rawType), value.ToString()) { }

        public TypedValue(DataType type, string serializedValue)
        {
            Type = type;
            _serializedValue = serializedValue;
            Parse();
        }

        public TypedValue(DataType type, object value)
        {
            try
            {
                if (value == null)
                {
                    Type = type;
                    Value = _serializedValue = null;
                }
                else if
                (
                       type == DataType.Boolean && !(value is bool) ||
					   type == DataType.DateTime && !(value is DateTime) ||
					   type == DataType.Date && !(value is DateTime) ||
					   type == DataType.Time && !(value is TimeSpan) ||
					   type == DataType.Number && !(value is decimal) ||
                       type == DataType.Text && !(value is string)
                )
                    throw new Exception("Invalid class " + value.GetType().Name + " for DataType " + type.ToString());
                else
                {
                    Type = type;
                    Value = value;
                    _serializedValue = value.ToString();
                }
            }
            catch (Exception exception)
            {
                Type = DataType.Error;
                Value = exception.Message;
            }
        }

        private void Parse()
        {
            try
            {
                Value =
                    string.IsNullOrWhiteSpace(_serializedValue) ? null :
                    Type == DataType.Boolean ? "true".Equals(_serializedValue, StringComparison.InvariantCultureIgnoreCase) :
					Type == DataType.DateTime ? DateTime.ParseExact(_serializedValue, new[] { "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm" }, null, System.Globalization.DateTimeStyles.None) :
					Type == DataType.Date ? DateTime.ParseExact(_serializedValue, new[] { "dd/MM/yyyy" }, null, System.Globalization.DateTimeStyles.None) :
					Type == DataType.Time ? TimeSpan.ParseExact(_serializedValue, new[] { "hh\\:mm" }, null) :
					Type == DataType.Number ? decimal.Parse(_serializedValue) :
                    Type == DataType.Error ? _serializedValue :
                    (object)_serializedValue;
            }
            catch (Exception exception)
            {
                Type = DataType.Error;
                Value = _serializedValue = exception.Message;
            }
        }

	    public DataType Type { get; private set; }

	    private string _serializedValue;
        public string SerializedValue
        {
            get => _serializedValue;
	        set
            {
                _serializedValue = value;
                Parse();
            }
        }

        public object Value
        {
            get; private set;
        }

        public bool IsNull => Value == null;

	    public bool BooleanValue => (bool)(Value ?? false);

	    public decimal NumberValue => (decimal)(Value ?? 0);

	    public DateTime DateTimeValue => (DateTime)(Value ?? DateTime.MinValue);

	    public TimeSpan TimeSpanValue => (TimeSpan)(Value ?? 0);

	    public string TextValue => (string)Value;

	    public string ErrorValue => (string)Value;
    }
}
