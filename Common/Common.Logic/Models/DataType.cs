using System;
using System.Collections.Generic;
using System.Globalization;
using CheckListManager.Common.Core.Helpers;
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

		public static bool operator ==(DataType a, DataType b) => a.Equals(b);

        public static bool operator !=(DataType a, DataType b) => !a.Equals(b);

		public override bool Equals(object obj) => obj is DataType type && type._rawValue == _rawValue;

		public override int GetHashCode() => _rawValue.GetHashCode();

		public override string ToString() => TypeName;

		public bool TryParse(string value, out object result)
		{
            result = null;
            if (string.IsNullOrEmpty(value)) return true;

            try
            {
                switch (_rawValue)
                {
                    case DataTypeEnum.Boolean:
                        result = "true".Equals(value, StringComparison.InvariantCultureIgnoreCase) ? true : "false".Equals(value, StringComparison.InvariantCultureIgnoreCase) ? false : default(bool?);
                        return result != null;
                    case DataTypeEnum.Number:
                        result = decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalResult) ? decimalResult : default(decimal?);
                        return result != null;
                    case DataTypeEnum.Date:
                        result = DateTimeOffset.TryParseExact(value, DateTimeOffsetHelper.DateFormat, null, DateTimeStyles.AssumeUniversal, out var dateTimeOffsetResult) ? dateTimeOffsetResult : default(DateTimeOffset?);
                        return result != null;
                    case DataTypeEnum.DateTime:
                        result = DateTimeOffset.TryParseExact(value, DateTimeOffsetHelper.DateTimeFormats, null, DateTimeStyles.AssumeUniversal, out dateTimeOffsetResult) ? dateTimeOffsetResult : default(DateTimeOffset?);
                        return result != null;
                    case DataTypeEnum.Time:
                        result = TimeSpan.TryParseExact(value, new string[] { "hh\\:mm", "hh\\:mm\\:ss" }, null, out var timeSpanResult) ? timeSpanResult : default(TimeSpan?);
                        return result != null;
                    default:
                        result = value;
                        return true;
                }
			}
			catch
			{
				return false;
			}
		}

		public static DataType Text = new DataType(DataTypeEnum.Text, "Text", typeof(string));
		public static DataType Boolean = new DataType(DataTypeEnum.Boolean, "Boolean", typeof(bool?));
		public static DataType Number = new DataType(DataTypeEnum.Number, "Number", typeof(decimal?));
		public static DataType DateTime = new DataType(DataTypeEnum.DateTime, "DateTime", typeof(DateTimeOffset?));
		public static DataType Date = new DataType(DataTypeEnum.Date, "Date", typeof(DateTimeOffset?));
		public static DataType Time = new DataType(DataTypeEnum.Time, "Time", typeof(TimeSpan?));
        public static DataType Array = new DataType(DataTypeEnum.Array, "Array", typeof(List<TypedValue>));
        public static DataType Error = new DataType(DataTypeEnum.Error, "Error", typeof(string));
        public static DataType Unknown = new DataType(DataTypeEnum.Unkown, null, null);

        public static DataType FromName(string name, bool strict = false)
        {
            return
                name == "Boolean" ? Boolean :
                name == "Number" ? Number :
				name == "DateTime" ? DateTime :
				name == "Date" ? Date :
				name == "Time" ? Time :
                name == "Array" ? Array :
				name == "Error" ? Error :
				name == "Text" ? Text : 
                name == "Unknown" || !strict ? Unknown :
                throw new InvalidOperationException($"DataType {name} not valid");
        }

        public enum DataTypeEnum
		{
			Text, Boolean, Number, DateTime, Date, Time, Array, Error, Unkown
		}
	}
}
