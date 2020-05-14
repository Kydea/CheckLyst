using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.FTP
{

    public class SFTPClient : IFTPClient
    {
        private ConnectionInfo GetConnectionInfo(string Host, int Port, string UserName, string Key)
        {
            using (var MemoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Key)))
            {
                var keyFiles = new[] { new PrivateKeyFile(MemoryStream) };
                return new ConnectionInfo(Host, Port, UserName, new PrivateKeyAuthenticationMethod(UserName, keyFiles));
            }
        }

        public Task ActionAsync(byte[] Bytes, string Path, string FileName, FTPConnectionSettings Settings, FTPMethods Method)
        {
            if (string.IsNullOrEmpty(Path))
                Path = string.Empty;
            else
            {
                if (!Path.EndsWith("/")) Path += "/";
                if (!Path.StartsWith("/")) Path = "/" + Path;
            }

            FileName = FileName?
                .Replace('\\', '-').Replace('/', '-').Replace('|', '-').Replace('>', '-').Replace('<', '-')
                .Replace(':', '-').Replace('*', '-').Replace('?', '-')
                .Replace('"', '\'') ?? "";

            using (SftpClient client = new SftpClient(GetConnectionInfo(Settings.Host, Settings.Port, Settings.UserName, null)))
            {
                client.Connect();
                if (Method == FTPMethods.Upload)
                    using (var MemoryStream = new MemoryStream(Bytes))
                        client.UploadFile(MemoryStream, $"{Path}/{FileName}", false);

            }

            return Task.CompletedTask;
        }

        public Task<string> TestAsync(FTPConnectionSettings Settings)
        {
            var Path = Settings.Folder;
            if (string.IsNullOrEmpty(Path))
                Path = string.Empty;
            else
            {
                if (!Path.EndsWith("/")) Path += "/";
                if (!Path.StartsWith("/")) Path = "/" + Path;
            }

            using (SftpClient client = new SftpClient(GetConnectionInfo(Settings.Host, Settings.Port, Settings.UserName, null)))
            {
                client.Connect();
                return Task.FromResult(string.Join(Environment.NewLine, client.ListDirectory(Path).Select(f => f.Name)));
            }
        }
    }
}