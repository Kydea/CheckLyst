using CheckListManager.Prototypes.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class PrototypeRequest
	{
		public long Id { get; set; }
		public int Version { get; set; }
        public Prototype CheckList { get; set; }
    }
}
