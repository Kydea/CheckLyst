using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Terminal.FTP
{

    public class FTPClient
    {
        public enum Methods { Upload, Deletion };

        public async Task ActionAsync(byte[] Bytes, string FileName, FTPConnectionSettings Settings, Methods Method)
        {
            String Status = "(unset)";

            var Uri = $"ftp://{Settings.Host}:{Settings.Port}/{System.Uri.EscapeDataString(FileName)}.pdf";
            FtpWebRequest FtpWebRequest = (FtpWebRequest)WebRequest.Create(Uri);
            FtpWebRequest.Credentials = new NetworkCredential(Settings.UserName, Settings.Password);
            FtpWebRequest.Method = Method == Methods.Deletion ? WebRequestMethods.Ftp.DeleteFile : WebRequestMethods.Ftp.UploadFile;

            FtpWebRequest.UsePassive = Settings.UsePassive;
            FtpWebRequest.EnableSsl = Settings.EnableSSL;

            //if (???)
            //    ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            if (Method == Methods.Upload)
                using (Stream RequestStream = await FtpWebRequest.GetRequestStreamAsync())
                    await RequestStream.WriteAsync(Bytes, 0, Bytes.Count());

            using (FtpWebResponse Response = (FtpWebResponse)(await FtpWebRequest.GetResponseAsync()))
                Status = Response.StatusDescription;
        }
    }
}