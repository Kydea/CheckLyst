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
    public partial class Form1 : Form
    {
        public Form1()
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
                var Parameters = new RfcConfigParameters();
                if (destinationName == "RAE")
                {
                    Parameters.Add(RfcConfigParameters.Client, "050");
                    Parameters.Add(RfcConfigParameters.MessageServerHost, "dbcirae.jnj.com");
                    Parameters.Add(RfcConfigParameters.MessageServerService, "3601");
                    Parameters.Add(RfcConfigParameters.SystemID, "RAE");
                    Parameters.Add(RfcConfigParameters.LogonGroup, "COMMON");
                    Parameters.Add(RfcConfigParameters.User, "CPICITLABPI"); //DBL_PPRLATDW //CPICITLABPI //APEZZOLI
                    Parameters.Add(RfcConfigParameters.Password, "lat_web0"); //ITS_pw2014 //lat_web0 //Alessia,2
                    Parameters.Add(RfcConfigParameters.Language, "EN");
                }
                else if (destinationName == "RPE")
                {
                    Parameters.Add(RfcConfigParameters.Client, "050");
                    Parameters.Add(RfcConfigParameters.MessageServerHost, "dbcirpe.jnj.com");
                    Parameters.Add(RfcConfigParameters.MessageServerService, "3600");
                    Parameters.Add(RfcConfigParameters.SystemID, "RPE");
                    Parameters.Add(RfcConfigParameters.LogonGroup, "COMMON");
                    Parameters.Add(RfcConfigParameters.User, "APEZZOLI");
                    Parameters.Add(RfcConfigParameters.Password, "Alessia,2");
                    Parameters.Add(RfcConfigParameters.Language, "EN");
                }
                else
                    throw new ArgumentException();

                return Parameters;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            TestDetail();
        }

        private void TestHeader()
        {
            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            RfcDestination destination = RfcDestinationManager.GetDestination("RPE"); //RPE //RAE
            RfcRepository repository = destination.Repository;
            IRfcFunction function = repository.CreateFunction("BAPI_PROCORD_GET_LIST"); //BAPI_PROCORD_GET_LIST

            function.Invoke(destination);

            var Range = function.GetTable("ORDER_NUMBER_RANGE");
            Range.Append(1);
            Range.SetValue("SIGN", "I");
            Range.SetValue("OPTION", "BT");
            Range.SetValue("LOW", "20572408");
            Range.SetValue("HIGH", "20572408");

            function.SetValue("ORDER_NUMBER_RANGE", Range);

            var result = function.GetTable("ORDER_HEADER");
            function.Invoke(destination);
            var count = result.RowCount;
        }

        private void TestAFPO()
        {
            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            RfcDestination destination = RfcDestinationManager.GetDestination("RAE");
            RfcRepository repository = destination.Repository;
            IRfcFunction function = repository.CreateFunction("CO_SF_AFPO_READ");

            function.Invoke(destination);

            //function.SetValue("ORDER_NUMBER_RANGE", Range);

            var result = function.GetTable("ORDER_HEADER");
            function.Invoke(destination);

            var count = result.RowCount;
        }

        private void CompleteTest()
        {
            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            RfcDestination destination = RfcDestinationManager.GetDestination("RAE");
            RfcRepository repository = destination.Repository;
            var customerList = repository.CreateFunction("BAPI_CUSTOMER_GETLIST");

            customerList.Invoke(destination);

            IRfcTable idRange = customerList.GetTable("IdRange");
            idRange.SetValue("SIGN", "I");
            idRange.SetValue("OPTION", "BT");
            idRange.SetValue("LOW", "");
            idRange.SetValue("HIGH", "999999");

            //add selection range to customerList function to search for all customers
            customerList.SetValue("idrange", idRange);


            IRfcTable addressData = customerList.GetTable("AddressData");
            customerList.Invoke(destination);

            for (int cuIndex = 0; cuIndex < addressData.RowCount; cuIndex++)
            {
                addressData.CurrentIndex = cuIndex;
                IRfcFunction customerHierachy = repository.CreateFunction("BAPI_CUSTOMER_GETSALESAREAS");
                IRfcFunction customerDetail1 = repository.CreateFunction("BAPI_CUSTOMER_GETDETAIL1");
                IRfcFunction customerDetail2 = repository.CreateFunction("BAPI_CUSTOMER_GETDETAIL2");

                var CustomerNo = addressData.GetString("Customer");
                var CustomerName = addressData.GetString("Name");
                var Address = addressData.GetString("Street");
                var City = addressData.GetString("City");
                var StateProvince = addressData.GetString("Region");
                var CountryCode = addressData.GetString("CountryISO");
                var PostalCode = addressData.GetString("Postl_Cod1");

                customerDetail2.SetValue("CustomerNo", CustomerNo);
                customerDetail2.Invoke(destination);
                IRfcStructure generalDetail = customerDetail2.GetStructure("CustomerGeneralDetail");

                var Region = generalDetail.GetString("Reg_Market");
                var Industry = generalDetail.GetString("Industry");

                customerDetail1.Invoke(destination);
                IRfcStructure detail1 = customerDetail1.GetStructure("PE_CompanyData");

                var District = detail1.GetString("District");


                customerHierachy.Invoke(destination);
                customerHierachy.SetValue("CustomerNo", CustomerNo);
                customerHierachy.Invoke(destination);

                IRfcTable otherDetail = customerHierachy.GetTable("SalesAreas");

                string SalesOrg = "?", DistributionChannel = "?", Division = "?";

                if (otherDetail.RowCount > 0)
                {
                    SalesOrg = otherDetail.GetString("SalesOrg");
                    DistributionChannel = otherDetail.GetString("DistrChn");
                    Division = otherDetail.GetString("Division");
                }

                customerHierachy = null;
                customerDetail1 = null;
                customerDetail2 = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void TestDetail()
        {

            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            RfcDestination destination = RfcDestinationManager.GetDestination("RAE");
            RfcRepository repository = destination.Repository;
            IRfcFunction function = repository.CreateFunction("BAPI_PROCORD_GET_DETAIL");

            IRfcStructure Structure = function.GetStructure("ORDER_OBJECTS");
            Structure.SetValue("HEADER", "X");
            Structure.SetValue("POSITIONS", "X");
            Structure.SetValue("SEQUENCES", "X");
            Structure.SetValue("COMPONENTS", "X");
            Structure.SetValue("PHASES", "X");
            Structure.SetValue("PROD_REL_TOOLS", "X");
            Structure.SetValue("TRIGGER_POINTS", "X");

            //function.SetValue("NUMBER", "000020572408"); //linea H41
            //function.SetValue("NUMBER", "000020309146"); // TOPAMAX 25MG 60 SPRINKLE CAPS. 
            //function.SetValue("NUMBER", "000020601440"); // REMINYL 4MG 1 TABL. BULK JAPAN 
            function.SetValue("NUMBER", "000062015428"); // Erdafitinib (FGFRi 493) IR 5 mg FCT

            IRfcStructure Return = function.GetStructure("RETURN");
            var Message = Return.GetString("MESSAGE");
            var header = function.GetTable("HEADER");
            var positions = function.GetTable("POSITION");
            var sequences = function.GetTable("SEQUENCE");
            var components = function.GetTable("COMPONENT");
            var phase = function.GetTable("PHASE");
            var prod_rel_tools = function.GetTable("PROD_REL_TOOL");
            var trigger_points = function.GetTable("TRIGGER_POINT");
            function.Invoke(destination);

            var value = new Dictionary<string, string>();
            value["Tipo"] = header.GetString("PROD_SCHED_PROFILE");
            value["PO Number"] = header.GetString("ORDER_NUMBER");
            value["Linea"] = ""; //H125
            value["Operation"] = "100";
            value["Material Number"] = header.GetString("MATERIAL");
            value["Material Description"] = header.GetString("MATERIAL_TEXT");
            value["Batch"] = header.GetString("BATCH");
            value["DHT"] = "72";
            value["QtVersamentoPrevista"] = positions.GetDecimal("QUANTITY").ToString();
            value["QtaVersata"] = positions.GetDecimal("DELIVERED_QUANTITY").ToString();
            value["POEndDate"] = positions.GetDecimal("DELIVERED_QUANTITY").ToString();
            value["Note"] = positions.GetDecimal("DELIVERED_QUANTITY").ToString();

            FillGrid(dataGridView1, header);
            FillGrid(dataGridView2, positions);
            FillGrid(dataGridView3, sequences);
            FillGrid(dataGridView4, components);
            FillGrid(dataGridView5, phase);
        }

        private void FillGrid(DataGridView Grid, IRfcTable Data)
        {
            if (Data.RowCount > 0)
                for (var Index = 0; Index < Data.CurrentRow.ElementCount; ++Index)
                    Grid.Columns.Add($"Column{Index}", Data.CurrentRow[Index].Metadata.Name);

            for (var Row = 0; Row < Data.RowCount; ++Row)
            {
                Data.CurrentIndex = Row;
                Grid.Rows.Add();
                for (var Column = 0; Column < Data.CurrentRow.ElementCount; ++Column)
                    Grid[Column, Row].Value = Data.CurrentRow[Column].GetString();
            }
        }

        private void TestRFCTable()
        {
            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            RfcDestination destination = RfcDestinationManager.GetDestination("RAE");
            RfcRepository repository = destination.Repository;
            IRfcFunction function = repository.CreateFunction("RFC_READ_TABLE");
            function.SetValue("QUERY_TABLE", "AFKO");
            function.Invoke(destination);
            var result = function.GetTable("ORDER_HEADER");
            var count = result.RowCount;
        }

        private void TestMaterial()
        {
            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            RfcDestination destination = RfcDestinationManager.GetDestination("RAE");
            RfcRepository repository = destination.Repository;
            IRfcFunction function = repository.CreateFunction("BAPI_MATERIALGROUP_GET_LIST");
            function.SetValue("SHORTDESCRIPTIONFLAG", "*"); //0020572077 //0002048652
            function.Invoke(destination);
            var result = function.GetTable("SHORTDESCRIPTIONS");
            var count = result.RowCount;
        }

        private void TestPO()
        {
            RfcDestinationManager.RegisterDestinationConfiguration(new Configuration());
            RfcDestination destination = RfcDestinationManager.GetDestination("RAE");
            RfcRepository repository = destination.Repository;
            IRfcFunction function = repository.CreateFunction("BAPI_PO_GET_LIST");
            function.Invoke(destination);
            var result = function.GetTable("PO_ITEMS");
            var count = result.RowCount;
        }

    }
}

