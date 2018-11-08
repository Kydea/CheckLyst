using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace CheckListManager.ExternalSources.Logic.SQL
{
    public class Source : SourceBase
    {
        private SQLParameters SQLParameters { get; set; }
        private DbConnection Connection;
        private DbCommand Command;
        private DbDataAdapter DataAdapter;

        public override void Parse(string Data)
        {
            SQLParameters = JsonConvert.DeserializeObject<SQLParameters>(Data);
        }

        private DbConnection GetConnection()
        {
            return
                SQLParameters.DBMS == "Oracle" ?
                    (DbConnection)new OracleConnection(SQLParameters.ConnectionString) :
                    new SqlConnection(SQLParameters.ConnectionString);
        }

        private DbCommand GetCommand(DbConnection Connection)
        {
            return
                SQLParameters.DBMS == "Oracle" ?
                    (DbCommand)new OracleCommand(SQLParameters.Query, (OracleConnection)Connection) :
                    new SqlCommand(SQLParameters.Query, (SqlConnection)Connection);
        }

        private DbDataAdapter GetDataAdapter(DbCommand Command)
        {
            return
                SQLParameters.DBMS == "Oracle" ?
                    (DbDataAdapter)new OracleDataAdapter((OracleCommand)Command) :
                    new SqlDataAdapter((SqlCommand)Command);
        }

        public DbParameter GetDbParameter(string Name, object Value)
        {
            return
                SQLParameters.DBMS == "Oracle" ?
                    (DbParameter)new OracleParameter(Name, Value) :
                    new SqlParameter(Name, Value);
        }

        public string GetDefaultParameterValue(Parameter Parameter)
        {
            return Parameter.ParameterType == "Text" ? "" : Parameter.ParameterType == "Number" ? "-1" : "01/01/1900";
        }

        public void ValidateParameter(Parameter Parameter)
        {
            //we carrently accept only text parameter
            if (Parameter.ParameterType != "Text")
                throw new Exception("Invalid parameter type: " + Parameter.ParameterType);
        }

        private DataSet LoadData(Dictionary<string, string> Values, int FirstRowIndex, int MaxRowsCount)
        {
            var DataSet = new DataSet();

            foreach (var Parameter in Parameters)
                Command.Parameters.Add(GetDbParameter(Parameter.Name, Values?[Parameter.Name] ?? GetDefaultParameterValue(Parameter)));
            DataAdapter.Fill(DataSet, 0, 1, "Source");

            return DataSet;
        }

        private void Setup()
        {
            var DataSet = LoadData(null, 0, 0);
            if (DataSet.Tables.Count == 0) throw new Exception("The query returns no table");
            if (DataSet.Tables.Count > 1) throw new Exception("The query returns more than one table");
            foreach (var Parameter in Parameters)
                ValidateParameter(Parameter);
            Columns = new List<string>(DataSet.Tables[0].Columns.Count);
            foreach (var Column in DataSet.Tables[0].Columns)
                Columns.Add(((DataColumn)Column).ColumnName);
        }

        public override void Open()
        {
            Connection = GetConnection();
            Command = GetCommand(Connection);
            DataAdapter = GetDataAdapter(Command);
        }

        public override void Dispose()
        {
            DataAdapter.Dispose();
            Command.Dispose();
            Connection.Dispose();
        }

        public override void Test()
        {
            Setup();
            LoadData(null, 0, 0);
        }

        public override IEnumerable<List<string>> GetData(Dictionary<string, string> Parameters, int Page, string DecimalSeparator)
        {
            Setup();
            //mettere formattazione invece che tostring generico sui campi
            return LoadData(Parameters, Page * PAGE_SIZE, PAGE_SIZE).Tables[0].AsEnumerable().Select(r => r.ItemArray.Select(v => v.ToString()).ToList()); 
        }
    }
}
