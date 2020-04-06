using System;
using System.Collections.Generic;
using System.Globalization;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Models
{
	public struct DataType
	{
		private readonly Type _actualType;
        private readonly DataTypeEnum _rawValue;
        public string TypeName { get; }

		private DataType(DataTypeEnum rawValue, string typeName, Type actualType)
		{
			_actualType = actualType;
			_rawValue = rawValue;
			TypeName = typeName;
		}

		public object FromJson(JToken token, JsonSerializer jsonSerializer)
			=> token.ToObject(_actualType, jsonSerializer);

		public static bool operator ==(DataType a, DataType b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(DataType a, DataType b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return obj is DataType type && type._rawValue == _rawValue;
		}

		public override int GetHashCode()
		{
			return _rawValue.GetHashCode();
		}

		public override string ToString() => TypeName;

		public bool Validate(string value)
		{
			try
			{
				if (string.IsNullOrEmpty(value)) return true;

                if (_rawValue == DataTypeEnum.Boolean)
	                return "true".Equals(value, StringComparison.InvariantCultureIgnoreCase) || "false".Equals(value, StringComparison.InvariantCultureIgnoreCase);
				if (_rawValue == DataTypeEnum.Number)
					return decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _);
                if (_rawValue == DataTypeEnum.Date)
                    return DateTimeOffset.TryParseExact(value, new[] { "yyyy-MM-dd" }, null, DateTimeStyles.AssumeUniversal, out _);
                if (_rawValue == DataTypeEnum.DateTime)
					return DateTimeOffset.TryParseExact(value, new[] { "yyyy-MM-dd HH:mm" }, null, DateTimeStyles.AssumeUniversal, out _);
				if (_rawValue == DataTypeEnum.Time)
					return TimeSpan.TryParseExact(value, "hh\\:mm", null, out _);

				return true;
			}
			catch
			{
				return false;
			}
		}

		public static DataType Text = new DataType(DataTypeEnum.Text, "Text", typeof(string));
		public static DataType Boolean = new DataType(DataTypeEnum.Boolean, "Boolean", typeof(bool));
		public static DataType Number = new DataType(DataTypeEnum.Number, "Number", typeof(decimal));
		public static DataType DateTime = new DataType(DataTypeEnum.DateTime, "DateTime", typeof(DateTimeOffset));
		public static DataType Date = new DataType(DataTypeEnum.Date, "Date", typeof(DateTimeOffset));
		public static DataType Time = new DataType(DataTypeEnum.Time, "Time", typeof(TimeSpan));
        public static DataType Array = new DataType(DataTypeEnum.Array, "Array", typeof(List<TypedValue>));
        public static DataType Error = new DataType(DataTypeEnum.Error, "Error", typeof(string));

        public static DataType FromName(string name)
        {
            return
                name == "Boolean" ? Boolean :
                name == "Number" ? Number :
				name == "DateTime" ? DateTime :
				name == "Date" ? Date :
				name == "Time" ? Time :
                name == "Array" ? Array :
				name == "Error" ? Error :
                Text;
        }

        public enum DataTypeEnum
		{
			Text, Boolean, Number, DateTime, Date, Time, Array, Error
		}
	}
}
