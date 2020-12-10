using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Npgsql;
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
        private const string ORACLE = "Oracle", POSTGRES = "Postgres", SQL_SERVER = "SQL Server";
        private SQLParameters SQLParameters { get; set; }

        public override void Parse(string Data)
        {
            SQLParameters = JsonConvert.DeserializeObject<SQLParameters>(Data);
            Parameters = SQLParameters.QueryParameters ?? new List<Parameter>(0);
        }

        private string GetConnectionString()
        {
            var ConnectionString =
                SQLParameters.DBMS == "Oracle" ?
                    (DbConnectionStringBuilder)new OracleConnectionStringBuilder() :
                    new SqlConnectionStringBuilder();

            ConnectionString.ConnectionString = SQLParameters.ConnectionString;
            ConnectionString["password"] = Password;
            return ConnectionString.ConnectionString;
        }

        private DbConnection GetConnection()
        {
            return
                SQLParameters.DBMS == ORACLE ? new OracleConnection(GetConnectionString()) :
                SQLParameters.DBMS == POSTGRES ? (DbConnection)new NpgsqlConnection(GetConnectionString()) :
                    new SqlConnection(GetConnectionString());
        }

        private DbCommand GetCommand(DbConnection Connection)
        {
            return
                SQLParameters.DBMS == ORACLE ? new OracleCommand(SQLParameters.Query, (OracleConnection)Connection) :
                SQLParameters.DBMS == POSTGRES ? (DbCommand)new NpgsqlCommand(SQLParameters.Query, (NpgsqlConnection)Connection) :
                    new SqlCommand(SQLParameters.Query, (SqlConnection)Connection);
        }

        private DbDataAdapter GetDataAdapter(DbCommand Command)
        {
            return
                SQLParameters.DBMS == ORACLE ? new OracleDataAdapter((OracleCommand)Command) :
                SQLParameters.DBMS == POSTGRES ? (DbDataAdapter)new NpgsqlDataAdapter((NpgsqlCommand)Command) :
                    new SqlDataAdapter((SqlCommand)Command);
        }

        private DbParameter GetDbParameter(string Name, object Value)
        {
            return
                SQLParameters.DBMS == ORACLE ? new OracleParameter(Name, Value) :
                SQLParameters.DBMS == POSTGRES ? (DbParameter)new NpgsqlParameter(Name, Value) :
                    new SqlParameter(Name, Value);
        }

        private string GetDefaultParameterValue(Parameter Parameter)
        {
            return Parameter.ParameterType == "Text" ? "" : Parameter.ParameterType == "Number" ? "-1" : "01/01/1900";
        }

        private void ValidateParameter(Parameter Parameter)
        {
            //we currently accept only text parameter
            if (Parameter.ParameterType != "Text")
                throw new Exception("Invalid parameter type: " + Parameter.ParameterType);
        }

        private string Format(object Value, string DecimalSeparator, string TimeZone)
        {
            return
                Value == null || Value == DBNull.Value ? string.Empty :
                Value is decimal DecimalValue ? Format(DecimalValue, DecimalSeparator) :
                Value is DateTime DateTimeValue ? Format(DateTimeValue) :
                Value is DateTimeOffset DateTimeOffsetValue ? Format(DateTimeOffsetValue, TimeZone) :
                Value.ToString();
        }

        private DataSet LoadData(Dictionary<string, string> Values, int FirstRowIndex, int MaxRowsCount)
        {
            using (var Connection = GetConnection())
            using (var Command = GetCommand(Connection))
            using (var DataAdapter = GetDataAdapter(Command))
            {
                var DataSet = new DataSet();

                foreach (var Parameter in Parameters)
                    Command.Parameters.Add(GetDbParameter(Parameter.Name, Values?[Parameter.Name] ?? GetDefaultParameterValue(Parameter)));
                DataAdapter.Fill(DataSet, FirstRowIndex, MaxRowsCount, "Source");

                return DataSet;
            }
        }

        public override void Open()
        {
        }

        public override void Dispose()
        {
        }

        public override void Test()
        {
            var DataSet = LoadData(null, 0, 0);
            if (DataSet.Tables.Count == 0) throw new Exception("The query returns no table");
            if (DataSet.Tables.Count > 1) throw new Exception("The query returns more than one table");
            foreach (var Parameter in Parameters)
                ValidateParameter(Parameter);
        }

        public override void PrepareMetaData()
        {
            var DataSet = LoadData(null, 0, 0);
            Columns = new List<string>(DataSet.Tables[0].Columns.Count);
            foreach (var Column in DataSet.Tables[0].Columns)
                Columns.Add(((DataColumn)Column).ColumnName);
        }

        public override IEnumerable<List<string>> GetData(Dictionary<string, string> Parameters, int Page, string DecimalSeparator, string TimeZone)
        {
            //mettere formattazione invece che tostring generico sui campi
            return LoadData(Parameters, Page * PAGE_SIZE, PAGE_SIZE).Tables[0].AsEnumerable().Select(r => r.ItemArray.Select(v => Format(v, DecimalSeparator, TimeZone)).ToList());
        }
    }
}
