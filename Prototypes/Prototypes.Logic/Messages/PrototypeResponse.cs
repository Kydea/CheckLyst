using CheckListManager.Prototypes.Logic.Models.CheckLists;
using System;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class PrototypeResponse
	{
		public long Id { get; set; }
		public int Version { get; set; }
		public long PrototypeType { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public string State { get; set; }
        public Prototype CheckList { get; set; }

        //TODO add properties from DB
    }
}
