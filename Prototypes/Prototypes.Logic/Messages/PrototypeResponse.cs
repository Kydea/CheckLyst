using CheckListManager.Prototypes.Logic.Models.CheckLists;
using System;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class PrototypeResponse
	{
		public long ID { get; set; }
		public int Version { get; set; }
		public long PrototypeType { get; set; }
        public Prototype CheckList { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
