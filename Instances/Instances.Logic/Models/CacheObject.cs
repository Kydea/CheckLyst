using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models
{
	public class CacheObject
	{
		public string Code { get; set; }
		public int Prototype { get; set; }
		public int Version { get; set; }
		public Instance Instance { get; set; }

		public int? Id { get; set; }
		public int? Year { get; set; }
		public int? Number { get; set; }

		public string Description { get; set; }
		public string InstanceState { get; set; }
		public string PrototypeCode { get; set; }
	}
}