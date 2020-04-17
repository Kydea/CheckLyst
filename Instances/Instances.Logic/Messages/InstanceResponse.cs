using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class InstanceResponse
	{
		public long? Id { get; set; }
		public string Code { get; set; }
        public int? Year { get; set; }
        public int? Number { get; set; }
        public string State { get; set; }
        public long Department { get; set; }
        public string DepartmentName { get; set; }
        public long PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public long Prototype { get; set; }
        public string PrototypeName { get; set; }
        public string PrototypeDescription { get; set; }
        public int Version { get; set; }
        public string VersionState { get; set; }

        public dynamic CheckList { get; set; }
	}
}
