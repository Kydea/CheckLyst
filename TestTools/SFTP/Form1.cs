using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFTP
{
    //https://github.com/sshnet/SSH.NET/

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Output(string Text = null)
        {
            OutputTextBox.AppendText(Text + Environment.NewLine);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                Output("Connecting");
                ListFiles();
                UploadFile();
                DeleteFile();
                Output("Finish");
                Output();
            }
            catch (Exception Exception)
            {
                Output();
                Output("Error");
                Output(Exception.Message);
                Output();
                Output(Exception.StackTrace);
            }

        }

        private ConnectionInfo GetConnectionInfo()
        {
            string host = @"yourSftpServer.com";
            int port = 22;
            string username = "root";
            string password = @"p4ssw0rd";

            PrivateKeyFile keyFile = new PrivateKeyFile(@"path/to/OpenSsh-RSA-key.ppk");
            var keyFiles = new[] { keyFile };

            return new ConnectionInfo(
                host,
                port,
                username,
                new PasswordAuthenticationMethod(username, password),
                new PrivateKeyAuthenticationMethod("rsa.key"),
                new PrivateKeyAuthenticationMethod(username, keyFiles)
            );
        }

        private void ListFiles()
        {
            string remoteDirectory = "/some/example/directory";
            using (SftpClient client = new SftpClient(GetConnectionInfo()))
            {
                client.Connect();

                var files = client.ListDirectory(remoteDirectory);

                Output();
                foreach (var file in files)
                    Output(file.Name);
            }
        }

        private void DeleteFile()
        {
            string pathRemoteFileToDelete = "/var/www/vhosts/folder/somefile.txt";

            using (SftpClient client = new SftpClient(GetConnectionInfo()))
            {
                client.Connect();
                Output();
                client.DeleteFile(pathRemoteFileToDelete);
                Output("Deleted");
            }
        }

        private void UploadFile()
        {
            string remotePath = "/var/www/vhosts/folder/somefile.txt";

            using (SftpClient client = new SftpClient(GetConnectionInfo()))
            {
                client.Connect();

                Output();
                client.UploadFile(new FileStream("local file", FileMode.Open), remotePath, false, e => {
                    Output("Callback");
                });
                Output("Uploaded");
            }
        }

        private void StressTest_Click(object sender, EventArgs e)
        {
            DateTime
                initialDateTime = DateTime.Now,
                dateTime = DateTime.Now;

            Output();
            for (var index = 0; index < 5000; ++index)
            {
                UploadFile();
                DeleteFile();
                Output($"Elapsed time {(DateTime.Now - dateTime).TotalMilliseconds}");
                dateTime = DateTime.Now;
            }
            Output($"Totale time {(DateTime.Now - initialDateTime).TotalMilliseconds}");
        }
    }
}
