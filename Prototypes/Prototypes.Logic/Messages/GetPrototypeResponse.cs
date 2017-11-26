using CheckListManager.Prototypes.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class GetPrototypeResponse
	{
		public long Id { get; set; }
		public int Version { get; set; }

		public long PrototypeType { get; set; }

		//TODO add properties from DB

		public Prototype CheckList { get; set; }
	}
}
