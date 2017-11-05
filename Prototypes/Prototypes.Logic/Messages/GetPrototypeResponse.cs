using CheckListManager.Prototypes.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class GetPrototypeResponse
	{
		public int Id { get; set; }
		public int Version { get; set; }

		//TODO add properties from DB

		public Prototype Prototype { get; set; }
	}
}
