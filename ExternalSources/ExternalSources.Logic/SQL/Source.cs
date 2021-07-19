using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Globalization;

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
                SQLParameters.DBMS == ORACLE ? new OracleConnectionStringBuilder() :
                SQLParameters.DBMS == POSTGRES ? (DbConnectionStringBuilder)new NpgsqlConnectionStringBuilder() :
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

        private DbParameter GetDbParameter(string Name, string Type, object Value)
        {
            DbParameter Parameter =
                SQLParameters.DBMS == ORACLE ? 
                    Value == null ? new OracleParameter(Name, Type == "Boolean" ? OracleDbType.Decimal : Type == "Number" ? OracleDbType.Decimal : Type == "Date" ? OracleDbType.TimeStamp : Type == "DateTime" ? OracleDbType.TimeStamp : Type == "Time" ? OracleDbType.IntervalDS : OracleDbType.Varchar2) : 
                        new OracleParameter(Name, Value) :
                SQLParameters.DBMS == POSTGRES ? 
                    Value == null ? new NpgsqlParameter(Name, Type == "Boolean" ? NpgsqlTypes.NpgsqlDbType.Boolean : Type == "Number" ? NpgsqlTypes.NpgsqlDbType.Numeric : Type == "Date" ? NpgsqlTypes.NpgsqlDbType.Date : Type == "DateTime" ? NpgsqlTypes.NpgsqlDbType.Timestamp : Type == "Time" ? NpgsqlTypes.NpgsqlDbType.Time : NpgsqlTypes.NpgsqlDbType.Varchar) : 
                    new NpgsqlParameter(Name, Value) :
                Value == null ? new SqlParameter(Name, Type == "Boolean" ? SqlDbType.Bit : Type == "Number" ? SqlDbType.Decimal : Type == "Date" ? SqlDbType.Date : Type == "DateTime" ? SqlDbType.DateTime2 : Type == "Time" ? SqlDbType.Time : SqlDbType.VarChar) : 
                (DbParameter)new SqlParameter(Name, Value);

            if (Value == null) Parameter.Value = DBNull.Value;

            return Parameter;
        }

        private object GetParameterValue(string ParameterValue, string ParameterType)
        {
            object Value =
                SQLParameters.DBMS == ORACLE ?
                    ParameterType == "Boolean" ? "true".Equals(ParameterValue, StringComparison.InvariantCultureIgnoreCase) ? "true" : "false" :
                    ParameterType == "Number" ? string.IsNullOrEmpty(ParameterValue) ? default(decimal?) : decimal.TryParse(ParameterValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalResult) ? decimalResult : default(decimal?) :
                    ParameterType == "Date" ? DateTime.TryParseExact(ParameterValue, "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal, out var dateTimeResult) ? dateTimeResult : default(DateTime?) :
                    ParameterType == "DateTime" ? DateTime.TryParseExact(ParameterValue, new string[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss" }, null, DateTimeStyles.AssumeUniversal, out dateTimeResult) ? dateTimeResult : default(DateTime?) :
                    ParameterType == "Time" ? TimeSpan.TryParseExact(ParameterValue, new string[] { "hh\\:mm", "hh\\:mm\\:ss" }, null, out var timeSpanResult) ? timeSpanResult : default(TimeSpan?) :
                    (object)ParameterValue :
                ParameterType == "Boolean" ? "true".Equals(ParameterValue, StringComparison.InvariantCultureIgnoreCase) ? true : false :
                ParameterType == "Number" ? string.IsNullOrEmpty(ParameterValue) ? default(decimal?) : decimal.TryParse(ParameterValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimalResult) ? decimalResult : default(decimal?) :
                ParameterType == "Date" ? DateTimeOffset.TryParseExact(ParameterValue, "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal, out var dateTimeOffsetResult) ? dateTimeOffsetResult : default(DateTimeOffset?) :
                ParameterType == "DateTime" ? DateTimeOffset.TryParseExact(ParameterValue, new string[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss" }, null, DateTimeStyles.AssumeUniversal, out dateTimeOffsetResult) ? dateTimeOffsetResult : default(DateTimeOffset?) :
                ParameterType == "Time" ? TimeSpan.TryParseExact(ParameterValue, new string[] { "hh\\:mm", "hh\\:mm\\:ss" }, null, out timeSpanResult) ? timeSpanResult : default(TimeSpan?) :
                (object)ParameterValue;

            return Value;
        }

        private string Format(object Value, string DecimalSeparator, string TimeZone)
        {
            return
                Value == null || Value == DBNull.Value ? string.Empty :
                Value is decimal DecimalValue ? Format(DecimalValue, DecimalSeparator) :
                Value is DateTime DateTimeValue ? Format(DateTimeValue) :
                Value is DateTimeOffset DateTimeOffsetValue ? Format(DateTimeOffsetValue, TimeZone) :
                Value is TimeSpan TimeSpanValue ? Format(TimeSpanValue) :
                Value is bool BooleanValue ? BooleanValue ? "true" : "false" :
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
                {
                    var ParameterValue = Values != null && (Values.TryGetValue(Parameter.Name, out var value) || Parameter.Name.StartsWith("@") && Values.TryGetValue(Parameter.Name.Substring(1), out value)) ? value : null;
                    var Value = GetParameterValue(ParameterValue, Parameter.ParameterType);
                    Command.Parameters.Add(GetDbParameter(Parameter.Name, Parameter.ParameterType, Value));
                }
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

        public override void PrepareMetaData()
        {
            var DataSet = LoadData(null, 0, 0);
            Columns = new List<string>(DataSet.Tables[0].Columns.Count);
            foreach (var Column in DataSet.Tables[0].Columns)
                Columns.Add(((DataColumn)Column).ColumnName);
        }

        public override IEnumerable<List<string>> GetData(Dictionary<string, string> Parameters, int Page, string DecimalSeparator, string TimeZone)
        {
            return LoadData(Parameters, Page * PAGE_SIZE, PAGE_SIZE).Tables[0].AsEnumerable().Select(r => r.ItemArray.Select(v => Format(v, DecimalSeparator, TimeZone)).ToList());
        }
    }
}
