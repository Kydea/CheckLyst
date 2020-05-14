using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.FTP
{
    public static class FTPClientBuilder
    {
        public static IFTPClient Create(string Protocol) =>
            Protocol == "FTP" ? (IFTPClient)new FTPClient() :
            Protocol == "SFTP" ? new SFTPClient() :
            throw new ArgumentException();
    }
}