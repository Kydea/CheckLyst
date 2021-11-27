using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class GetPrototypeResponse
	{
        public string Name { get; set; }
        public string Description { get; set; }
        public long Department { get; set; }
        public string DepartmentName { get; set; }
        public long PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public string State { get; set; }
        public Instance CheckList { get; set; }
        public int? Version { get; set; }
	}
}
