using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class GetPrototypeResponse
	{
		public long Id { get; set; }
        public long Department { get; set; }
        public string DepartmentName { get; set; }
        public long PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public long Prototype { get; set; }
        public string PrototypeName { get; set; }
        public int Version { get; set; }
        public Instance CheckList { get; set; }
	}
}
