using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.FTP
{

    public interface IFTPClient
    {
        Task ActionAsync(byte[] Bytes, string Path, string FileName, FTPConnectionSettings Settings, FTPMethods Method);

        Task<string> TestAsync(FTPConnectionSettings Settings);
   }
}