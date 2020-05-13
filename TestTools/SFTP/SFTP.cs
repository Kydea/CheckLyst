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

    public partial class SFTP : Form
    {
        public SFTP()
        {
            InitializeComponent();
            PathTextBox.Text = $"/home/{UserTextBox.Text}";
        }

        private void Output(string Text = null)
        {
            OutputTextBox.AppendText(Text + Environment.NewLine);
        }

        private void ListFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                Output("Connecting");
                ListFiles();
                Output($"Finished in {(DateTime.Now - dateTime).TotalMilliseconds} ms");
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

        private void UploadFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                Output("Connecting");
                UploadFile();
                Output($"Finished in {(DateTime.Now - dateTime).TotalMilliseconds} ms");
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

        private void DeleteFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                Output("Connecting");
                DeleteFile();
                Output($"Finished in {(DateTime.Now - dateTime).TotalMilliseconds} ms");
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
            if (string.IsNullOrWhiteSpace(HostTextBox.Text)) throw new Exception("Provide a host");
            if (string.IsNullOrWhiteSpace(PortTextBox.Text)) throw new Exception("Provide a port");
            if (string.IsNullOrWhiteSpace(UserTextBox.Text)) throw new Exception("Provide a user");
            if (string.IsNullOrWhiteSpace(KeyTextBox.Text)) throw new Exception("Provide a key");
            if (string.IsNullOrWhiteSpace(PathTextBox.Text)) throw new Exception("Provide a remote path");

            if (!int.TryParse(PortTextBox.Text, out var port)) throw new Exception("Invalid port number");

            //var keyFile = @"C:\\Matematici\\TestSFTP.ppk";
            //var keyFiles = new[] { new PrivateKeyFile(@"C:\\Matematici\\TestSFTP.pem") };

            var keyFiles = new[] { new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(KeyTextBox.Text.Trim()))) };
            return new ConnectionInfo(
                HostTextBox.Text,
                port,
                UserTextBox.Text,
                //new PasswordAuthenticationMethod(username, password),
                //new PrivateKeyAuthenticationMethod(keyFile)
                new PrivateKeyAuthenticationMethod(UserTextBox.Text, keyFiles)
            );
        }

        private void ListFiles()
        {
            using (SftpClient client = new SftpClient(GetConnectionInfo()))
            {
                client.Connect();

                var files = client.ListDirectory(PathTextBox.Text);

                Output();
                foreach (var file in files)
                    Output(file.Name);
            }
        }

        private void UploadFile()
        {
            if (string.IsNullOrWhiteSpace(LocalFileNameTextBox.Text)) throw new Exception("Provide a local file");
            if (string.IsNullOrWhiteSpace(RemoteFileNameTextBox.Text)) throw new Exception("Provide a remote file name");

            using (SftpClient client = new SftpClient(GetConnectionInfo()))
            {
                client.Connect();

                Output();

                client.UploadFile(new FileStream(LocalFileNameTextBox.Text, FileMode.Open), $"{PathTextBox.Text}/{RemoteFileNameTextBox.Text}", false);
                Output("Uploaded");
            }
        }

        private void DeleteFile()
        {
            if (string.IsNullOrWhiteSpace(RemoteFileNameTextBox.Text)) throw new Exception("Provide a remote file name");

            using (SftpClient client = new SftpClient(GetConnectionInfo()))
            {
                client.Connect();

                Output();
                client.DeleteFile($"{PathTextBox.Text}/{RemoteFileNameTextBox.Text}");
                Output("Deleted");
            }
        }

        private void ClearOutputButton_Click(object sender, EventArgs e)
        {
            OutputTextBox.Clear();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                LocalFileNameTextBox.Text = openFileDialog.FileName;
                RemoteFileNameTextBox.Text = $"{new FileInfo(LocalFileNameTextBox.Text).Name}";
            }
        }
    }
}
