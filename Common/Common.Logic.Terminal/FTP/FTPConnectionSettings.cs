using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.FTP
{

    public class FTPConnectionSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UsePassive { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Folder { get; set; }
    }

}
