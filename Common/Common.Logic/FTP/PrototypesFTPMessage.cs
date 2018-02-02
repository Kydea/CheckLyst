using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.FTP
{
	public class PrototypesFTPMessage
	{
		public PrototypesFTPMessage()
		{
			
		}

		public PrototypesFTPMessage(long company, string path, long prototype, string prototypeName, int version)
		{
			Company = company;
            Prototype = prototype;
            PrototypeName = prototypeName;
            Version = version;
            Path = Path;
        }

		public long Company { get; set; }
		public long Prototype { get; set; }
		public string PrototypeName { get; set; }
		public int Version { get; set; }
        public string Path { get; set; }
	}
}
