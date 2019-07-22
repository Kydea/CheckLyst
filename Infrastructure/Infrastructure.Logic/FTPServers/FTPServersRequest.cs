using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.FTPServers
{
    public class FTPServersRequest : Request
    {
        public string Name { get; set; }
        public string Host { get; set; }
    }
}
