using System.Data;

namespace CheckListManager.Common.Data
{
	public class Parameter
	{
		public Parameter(string name, object value)
		{
			Name = name;
			Value = value;
		}

		public Parameter(string name, DbType dbType)
		{
			Name = name;
			DbType = dbType;
		}

		public string Name { get; }
		public object Value { get; }
		public DbType? DbType { get; }
	}
}
