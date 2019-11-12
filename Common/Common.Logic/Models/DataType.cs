using System;
using System.Collections.Generic;
using System.Globalization;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models
{
	public struct DataType
	{
		private readonly DataTypeEnum _rawValue;
		private readonly HashSet<Type> _compatibleTypes;

		[JsonIgnore]
		public Type SystemType { get; }

        public string TypeName { get; }

		private DataType(DataTypeEnum rawValue, Type systemType, string typeName) : this (rawValue, systemType, typeName, null)
		{
		}

		private DataType(DataTypeEnum rawValue, Type systemType, string typeName, params Type[] compatibleTypes)
		{
			_rawValue = rawValue;
			SystemType = systemType;

			_compatibleTypes = new HashSet<Type>(compatibleTypes ?? new Type[0]) { systemType };
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
			return obj is DataType && ((DataType)obj)._rawValue == _rawValue;
		}

		public override int GetHashCode()
		{
			return _rawValue.GetHashCode();
		}

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

		public static DataType Text = new DataType(DataTypeEnum.Text, typeof(string), "Text");
		public static DataType Boolean = new DataType(DataTypeEnum.Boolean, typeof(bool), "Boolean");
		public static DataType Number = new DataType(DataTypeEnum.Number, typeof(decimal), "Number", typeof(int), typeof(double), typeof(float));
		public static DataType DateTime = new DataType(DataTypeEnum.DateTime, typeof(DateTimeOffset), "DateTime");
		public static DataType Date = new DataType(DataTypeEnum.Date, typeof(DateTimeOffset), "Date");
		public static DataType Time = new DataType(DataTypeEnum.Time, typeof(TimeSpan), "Time");
        public static DataType Array = new DataType(DataTypeEnum.Array, typeof(IEnumerable<TypedValue>), "Array");
        public static DataType Error = new DataType(DataTypeEnum.Error, typeof(string), "Error");

		public static DataType FromType(Type type)
		{
			if (Text._compatibleTypes.Contains(type))
				return Text;
			if (Boolean._compatibleTypes.Contains(type))
				return Boolean;
			if (Number._compatibleTypes.Contains(type))
				return Number;
			if (DateTime._compatibleTypes.Contains(type))
				return DateTime;
			if (Date._compatibleTypes.Contains(type))
				return DateTime;
            if (Array._compatibleTypes.Contains(type))
                return Array;
            if (Time._compatibleTypes.Contains(type))
				return Time;

			throw new Exception("Type " + type.FullName + " not handled.");
		}

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

        private enum DataTypeEnum
		{
			Text, Boolean, Number, DateTime, Date, Time, Array, Error
		}
	}
}
