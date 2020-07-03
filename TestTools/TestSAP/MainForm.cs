using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestSAP
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public class Configuration : IDestinationConfiguration
        {
            public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

            public bool ChangeEventsSupported()
            {
                return false;
            }

            public RfcConfigParameters GetParameters(string destinationName)
            {
                return parameters;

                //var Parameters = new RfcConfigParameters();
                //if (destinationName == "RSV")
                //{
                //    Parameters.Add(RfcConfigParameters.Client, "100");
                //    Parameters.Add(RfcConfigParameters.MessageServerHost, "ITSGPSG2.jnj.com");
                //    Parameters.Add(RfcConfigParameters.MessageServerService, "3664");
                //    Parameters.Add(RfcConfigParameters.SystemID, "RSV");
                //    Parameters.Add(RfcConfigParameters.LogonGroup, "PSGA_ECC");
                //    Parameters.Add(RfcConfigParameters.User, "CHECKLYST");
                //    Parameters.Add(RfcConfigParameters.Password, "Test@12");
                //    Parameters.Add(RfcConfigParameters.Language, "EN");
                //}
                //else if (destinationName == "RQV")
                //{
                //    Parameters.Add(RfcConfigParameters.Client, "100");
                //    Parameters.Add(RfcConfigParameters.MessageServerHost, "dbciRQV.jnj.com");
                //    Parameters.Add(RfcConfigParameters.MessageServerService, "3650");
                //    Parameters.Add(RfcConfigParameters.SystemID, "RQV");
                //    Parameters.Add(RfcConfigParameters.LogonGroup, "PSGA_ECC");
                //    Parameters.Add(RfcConfigParameters.User, "checklist");
                //    Parameters.Add(RfcConfigParameters.Password, "Test@12");
                //    Parameters.Add(RfcConfigParameters.Language, "EN");
                //}
                //else if (destinationName == "RAE")
                //{
                //    Parameters.Add(RfcConfigParameters.Client, "050");
                //    Parameters.Add(RfcConfigParameters.MessageServerHost, "dbcirae.jnj.com");
                //    Parameters.Add(RfcConfigParameters.MessageServerService, "3601");
                //    Parameters.Add(RfcConfigParameters.SystemID, "RAE");
                //    Parameters.Add(RfcConfigParameters.LogonGroup, "COMMON");
                //    Parameters.Add(RfcConfigParameters.User, "CPICITLABPI"); //DBL_PPRLATDW //CPICITLABPI //APEZZOLI
                //    Parameters.Add(RfcConfigParameters.Password, "lat_web0"); //ITS_pw2014 //lat_web0 //Alessia,2
                //    Parameters.Add(RfcConfigParameters.Language, "EN");
                //}
                //else if (destinationName == "RPE")
                //{
                //    Parameters.Add(RfcConfigParameters.Client, "050");
                //    Parameters.Add(RfcConfigParameters.MessageServerHost, "dbcirpe.jnj.com");
                //    Parameters.Add(RfcConfigParameters.MessageServerService, "3600");
                //    Parameters.Add(RfcConfigParameters.SystemID, "RPE");
                //    Parameters.Add(RfcConfigParameters.LogonGroup, "COMMON");
                //    Parameters.Add(RfcConfigParameters.User, "APEZZOLI");
                //    Parameters.Add(RfcConfigParameters.Password, "Alessia,2");
                //    Parameters.Add(RfcConfigParameters.Language, "EN");
                //}
                //else if (destinationName == "RDG")
                //{
                //    Parameters.Add(RfcConfigParameters.Client, "100");
                //    Parameters.Add(RfcConfigParameters.MessageServerHost, "csRDG.jnj.com");
                //    Parameters.Add(RfcConfigParameters.MessageServerService, "3610");
                //    Parameters.Add(RfcConfigParameters.SystemID, "RDG");
                //    Parameters.Add(RfcConfigParameters.LogonGroup, "COMMON");
                //    Parameters.Add(RfcConfigParameters.User, "CR5ECRDGCHK");
                //    Parameters.Add(RfcConfigParameters.Password, "Welcome@20");
                //    Parameters.Add(RfcConfigParameters.Language, "EN");
                //}
                //else
                //    throw new ArgumentException();

                //return Parameters;
            }
        }

        private RfcDestination destination;
        private RfcRepository repository;
        private static RfcConfigParameters parameters;

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                var Login = new Login();
                if (Login.ShowDialog() == DialogResult.OK)
                {
                    parameters = new RfcConfigParameters
                    {
                        { RfcConfigParameters.Client, Login.Client.Text },
                        { RfcConfigParameters.MessageServerHost, Login.Host.Text },
                        { RfcConfigParameters.MessageServerService, Login.Service.Text },
                        { RfcConfigParameters.SystemID, Login.ID.Text },
                        { RfcConfigParameters.LogonGroup, Login.Group.Text },
                        { RfcConfigParameters.User, Login.User.Text },
                        { RfcConfigParameters.Password, Login.Password.Text },
                        { RfcConfigParameters.Language, Login.Language.Text }
                    };

                    //RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
                    RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
                    destination = RfcDestinationManager.GetDestination(Login.ID.Text);
                    Text = Text + " " + Login.ID.Text;
                    repository = destination.Repository;
                }
            }
            catch (Exception Exception)
            {
                MessageBox.Show(Exception.Message, "Error");
                Close();
            }
        }

        private void LoadData()
        {
            try
            {

                IRfcFunction function = repository.CreateFunction("BAPI_PROCORD_GET_DETAIL");

                IRfcStructure Structure = function.GetStructure("ORDER_OBJECTS");
                Structure.SetValue("HEADER", "X");
                Structure.SetValue("POSITIONS", "X");
                Structure.SetValue("SEQUENCES", "X");
                Structure.SetValue("COMPONENTS", "X");
                Structure.SetValue("PHASES", "X");
                Structure.SetValue("PROD_REL_TOOLS", "X");
                Structure.SetValue("TRIGGER_POINTS", "X");

                function.SetValue("NUMBER", textBox1.Text.PadLeft(12, '0'));

                IRfcStructure Return = function.GetStructure("RETURN");
                var Message = Return.GetString("MESSAGE");
                var Header = function.GetTable("HEADER");
                var Positions = function.GetTable("POSITION");
                var Sequences = function.GetTable("SEQUENCE");
                var Components = function.GetTable("COMPONENT");
                var Phase = function.GetTable("PHASE");
                var ProdRelTools = function.GetTable("PROD_REL_TOOL");
                var TriggerPoints = function.GetTable("TRIGGER_POINT");
                function.Invoke(destination);

                FillGrid(dataGridView1, Header);
                FillGrid(dataGridView2, Positions);
                FillGrid(dataGridView3, Sequences);
                FillGrid(dataGridView4, Components);
                FillGrid(dataGridView5, Phase);
                FillGrid(dataGridView6, ProdRelTools);
                FillGrid(dataGridView7, TriggerPoints);

                for (int Index = dataGridView8.Rows.Count - 1; Index > 0; --Index)
                    dataGridView8.Rows.RemoveAt(Index - 1);

                for (int Index = dataGridView8.Columns.Count - 1; Index >= 0; --Index)
                    dataGridView8.Columns.RemoveAt(Index);

                AddColumn(dataGridView8, "PO");
                AddColumn(dataGridView8, "Type");
                AddColumn(dataGridView8, "Line");
                AddColumn(dataGridView8, "Operation");
                AddColumn(dataGridView8, "Operation Description");
                AddColumn(dataGridView8, "Material Number");
                AddColumn(dataGridView8, "Material Description");
                AddColumn(dataGridView8, "Batch");
                AddColumn(dataGridView8, "Scheduled Quantity");
                AddColumn(dataGridView8, "Effective Quantity");
                AddColumn(dataGridView8, "DHT ?");
                AddColumn(dataGridView8, "PO End Date ?");
                AddColumn(dataGridView8, "Equipment");

                var Row = 0;
                for (var PhaseIndex = 0; PhaseIndex < Phase.RowCount; ++PhaseIndex)
                {
                    Phase.CurrentIndex = PhaseIndex;
                    //if ((Phase.GetString("OPERATION_NUMBER")?.EndsWith("00") ?? false) && Phase.GetString("DESCRIPTION") != "Checks")
                    //if (Phase.CurrentRow["OPERATION_NUMBER"].GetString()?.EndsWith("00") ?? false)
                    {
                        dataGridView8.Rows.Add();
                        dataGridView8["PO", Row].Value = Header.GetString("ORDER_NUMBER")?.TrimStart('0');
                        dataGridView8["Type", Row].Value = Header.GetString("PROD_SCHED_PROFILE");
                        dataGridView8["Line", Row].Value = Phase.GetString("RESOURCE");
                        dataGridView8["Operation", Row].Value = Phase.GetString("OPERATION_NUMBER");
                        dataGridView8["Operation Description", Row].Value = Phase.GetString("DESCRIPTION");
                        dataGridView8["Material Number", Row].Value = Header.GetString("MATERIAL")?.TrimStart('0');
                        dataGridView8["Material Description", Row].Value = Header.GetString("MATERIAL_TEXT");
                        dataGridView8["Batch", Row].Value = Header.GetString("BATCH");
                        dataGridView8["Scheduled Quantity", Row].Value = Positions.GetInt("QUANTITY").ToString();
                        dataGridView8["Effective Quantity", Row].Value = Positions.GetInt("DELIVERED_QUANTITY").ToString();
                        ++Row;
                    }
                }
            }
            catch (Exception Exception)
            {
                MessageBox.Show(Exception.Message, "Error");
            }
        }

        private void AddColumn(DataGridView Grid, string Label)
        {
            Grid.Columns.Add(Label, Label);
            Grid.Columns[Grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void FillGrid(DataGridView Grid, IRfcTable Data)
        {
            for (int Index = Grid.Rows.Count - 1; Index > 0; --Index)
                Grid.Rows.RemoveAt(Index - 1);

            for (int Index = Grid.Columns.Count - 1; Index >= 0; --Index)
                Grid.Columns.RemoveAt(Index);

            if (Data.RowCount > 0)
                for (var Index = 0; Index < Data.CurrentRow.ElementCount; ++Index)
                {
                    Grid.Columns.Add($"Column{Index}", Data.CurrentRow[Index].Metadata.Name.Replace('_', ' ').ToLower());
                    Grid.Columns[Index].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

            for (var Row = 0; Row < Data.RowCount; ++Row)
            {
                Data.CurrentIndex = Row;
                Grid.Rows.Add();
                for (var Column = 0; Column < Data.CurrentRow.ElementCount; ++Column)
                    Grid[Column, Row].Value = Data.CurrentRow[Column].GetString();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}

