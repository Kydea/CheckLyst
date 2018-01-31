using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.FTP
{
	public class InstancesFTPMessage
    {
		public InstancesFTPMessage()
		{
			
		}

		public InstancesFTPMessage(long company, string path, long id)
		{
			Company = company;
            ID = id;
            Path = Path;
        }

		public long Company { get; set; }
		public long ID { get; set; }
        public string Path { get; set; }
	}
}
