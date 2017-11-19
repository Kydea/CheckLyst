using CheckListManager.Instances.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models
{
	public class InstanceCacheObject
	{
		public string Code { get; set; }
		public int Prototype { get; set; }
		public int Version { get; set; }

		[JsonIgnore]
		public Instance Instance { get; set; }

		public long? Id { get; set; }
		public int? Year { get; set; }
		public int? Number { get; set; }

		public string Description { get; set; }
		public string InstanceState { get; set; }
		public string PrototypeCode { get; set; }
	}
}