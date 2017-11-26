using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class GetPrototypeResponse
	{
		public long Id { get; set; }
		public int Version { get; set; }

		public long PrototypeType { get; set; }

		//TODO add properties from DB

		public Instance CheckList { get; set; }
	}
}
