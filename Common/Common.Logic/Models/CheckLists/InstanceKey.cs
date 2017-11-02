using System;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public struct InstanceKey
	{
		public string Prototype { get; set; }
		public int Version { get; set; }
		public int Year { get; set; }
		public int Number { get; set; }

		public InstanceKey(string prototype, int version, int year, int number)
		{
			this.Prototype = prototype;
			this.Version = version;
			this.Year = year;
			this.Number = number;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return (obj is InstanceKey) && obj.ToString() == ToString();
		}

		public override string ToString()
		{
			return string.Format("{0}-{1}-{2}-{3}", Prototype, Version, Year, Number);
		}

		public static bool operator ==(InstanceKey a, InstanceKey b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(InstanceKey a, InstanceKey b)
		{
			return !a.Equals(b);
		}
	}
}