using System;
using CheckListManager.Common.Logic.Models.Expressions;

// ReSharper disable SuspiciousTypeConversion.Global

namespace CheckListManager.Common.Logic.Models
{
	public class RawValue
	{
		public string Value { get; set; }

		public RawValue() { }

		public RawValue(string value)
		{
			Value = value;
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return Value?.GetHashCode() ?? 0;
		}

		public override bool Equals(object obj)
		{
			return obj is RawValue && obj.ToString() == ToString();
		}

		public override string ToString()
		{
			return Value;
		}
    }
}