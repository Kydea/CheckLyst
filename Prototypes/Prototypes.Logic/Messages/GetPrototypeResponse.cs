using CheckListManager.Prototypes.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class GetPrototypeResponse
	{
		public int Id { get; set; }
		public int Version { get; set; }
        public string State { get; set; }
        public Prototype CheckList { get; set; }
	}
}
