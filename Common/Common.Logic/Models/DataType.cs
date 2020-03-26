using System;
using System.Collections.Generic;
using System.Globalization;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models
{
	public struct DataType
	{
		public DataTypeEnum RawValue { get; set; }
        public string TypeName { get; set; }

		private DataType(DataTypeEnum rawValue, string typeName)
		{
			RawValue = rawValue;
			TypeName = typeName;
        }

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
			return obj is DataType && ((DataType)obj).RawValue == RawValue;
		}

		public override int GetHashCode()
		{
			return RawValue.GetHashCode();
		}

		public bool Validate(string value)
		{
			try
			{
				if (string.IsNullOrEmpty(value)) return true;

                if (RawValue == DataTypeEnum.Boolean)
	                return "true".Equals(value, StringComparison.InvariantCultureIgnoreCase) || "false".Equals(value, StringComparison.InvariantCultureIgnoreCase);
				if (RawValue == DataTypeEnum.Number)
					return decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _);
                if (RawValue == DataTypeEnum.Date)
                    return DateTimeOffset.TryParseExact(value, new[] { "yyyy-MM-dd" }, null, DateTimeStyles.AssumeUniversal, out _);
                if (RawValue == DataTypeEnum.DateTime)
					return DateTimeOffset.TryParseExact(value, new[] { "yyyy-MM-dd HH:mm" }, null, DateTimeStyles.AssumeUniversal, out _);
				if (RawValue == DataTypeEnum.Time)
					return TimeSpan.TryParseExact(value, "hh\\:mm", null, out _);

				return true;
			}
			catch
			{
				return false;
			}
		}

		public static DataType Text = new DataType(DataTypeEnum.Text, "Text");
		public static DataType Boolean = new DataType(DataTypeEnum.Boolean, "Boolean");
		public static DataType Number = new DataType(DataTypeEnum.Number, "Number");
		public static DataType DateTime = new DataType(DataTypeEnum.DateTime, "DateTime");
		public static DataType Date = new DataType(DataTypeEnum.Date, "Date");
		public static DataType Time = new DataType(DataTypeEnum.Time, "Time");
        public static DataType Array = new DataType(DataTypeEnum.Array, "Array");
        public static DataType Error = new DataType(DataTypeEnum.Error, "Error");

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
