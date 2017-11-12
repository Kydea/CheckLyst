using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class GetPrototypeResponse
	{
		public int Id { get; set; }
		public int Version { get; set; }

		//TODO add properties from DB

		public Instance CheckList { get; set; }
	}
}
