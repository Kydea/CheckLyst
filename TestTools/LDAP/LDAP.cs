using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LDAP
{
    public partial class LDAP : Form
    {
        public LDAP()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            QueryTextBox.Anchor = AnchorStyles.Right;
            ConnectionTypeComboBox.SelectedIndex = SecurityComboBox.SelectedIndex = 0;
        }

        private void Authenticate()
        {
            try
            {
                OutputTextBox.Clear();

                if (!int.TryParse(PortComboBox.Text, out var Port))
                    throw new Exception($"Invalid port");

                var LdapDirectoryIdentifier = new LdapDirectoryIdentifier(HostTextBox.Text, Port, true, false); //ldap: 389, ldaps: 636
                var NetworkCredential = new NetworkCredential(UserTextBox.Text, PasswordTextBox.Text);


                var LdapConnection = new LdapConnection(LdapDirectoryIdentifier, NetworkCredential, ConnectionTypeComboBox.SelectedIndex == 0 ? AuthType.Basic : AuthType.Kerberos);
                LdapConnection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback((Connection, Certificate) => true); //not necessary on janssen server, use only for test for security reasons

                if (SecurityComboBox.SelectedIndex == 0)
                {
                    LdapConnection.SessionOptions.SecureSocketLayer = true;
                    LdapConnection.SessionOptions.ProtocolVersion = 3;
                }
                else
                    LdapConnection.SessionOptions.StartTransportLayerSecurity(null);

                LdapConnection.Bind();

                OutputTextBox.AppendText($"User {UserTextBox.Text}/{PasswordTextBox.Text} is valid\r\n");

                if (QueryTextBox.Text == "")
                    OutputTextBox.AppendText($"No query provided\r\n");
                else 
                {
                    List<string> CodeParts = UserTextBox.Text.Split('\\').ToList();
                    if (CodeParts.Count == 1)
                        CodeParts = UserTextBox.Text.Split('@').Reverse().ToList();
                    if (CodeParts.Count != 2)
                        throw new Exception($"Invalid user code {UserTextBox.Text} use format domain\\user or user@domain");

                    OutputTextBox.AppendText($"Accessing in LDAP://{HostTextBox.Text} with {UserTextBox.Text}/{PasswordTextBox.Text}\r\n");
                    DirectoryEntry Root = new DirectoryEntry($"LDAP://{HostTextBox.Text}", UserTextBox.Text, PasswordTextBox.Text);

                    var Query = QueryTextBox.Text.Replace("##DOMAIN##", CodeParts[0]);
                    var Filter = $"(&(objectClass=user)(sAMAccountName={CodeParts[1]})(memberOf={Query}))";
                    OutputTextBox.AppendText($"Query:\n{Query}\r\nFilter:\n{Filter}\r\n");
                    var Searcher = new DirectorySearcher(Root)
                    {
                        Filter = Filter
                    };

                    var Results = Searcher.FindAll()?.Count;
                    OutputTextBox.AppendText($"{Results} item found");
                    if (Results <= 0)
                        throw new Exception($"User is not enabled");
                }
            }
            catch (SimpleException Exception)
            {
                MessageBox.Show($"{Exception.Message}", "Validation error");
            }
            catch (Exception Exception)
            {
                MessageBox.Show($"{Exception.Message}\r\n{Exception.StackTrace}", "LDAP Error");
            }

        }

        private void QueryButton_Click(object sender, EventArgs e)
        {
            Authenticate();
        }
    }
}
