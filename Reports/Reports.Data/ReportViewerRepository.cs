using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Reports.Data.Models;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Data
{
    public class ReportViewerRepository : Repository, IReportViewerRepository
    {
        public ReportViewerRepository(RelationalDatabaseEntities entities) : base(entities)
        {
            var DatabaseConnection = entities.Database.Connection;
        }

        private async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ReportDatabase"].ConnectionString;
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        private object GetDefaultParameterValue(Parameter Parameter)
        {
            return string.Empty;
        }

        private object GetDefaultParameterValue(string DataType) =>
            DataType == "Date" || DataType == "DateTime" ? new DateTime(2000, 1, 1) :
            DataType == "Number" ? 0 :
            DataType == "Boolean" ? false :
            (object)string.Empty;

        public async Task<DataTable> GetDataTableAsync(long company, string command, Dictionary<string, string> parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var Connection = await GetConnectionAsync(cancellationToken))
                using (var Command = new NpgsqlCommand(command, Connection))
                using (var DataAdapter = new NpgsqlDataAdapter(Command))
                using (var DataSet = new DataSet())
                {
                    Command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.Key, GetDefaultParameterValue(p.Value))).ToArray());
                    cancellationToken.ThrowIfCancellationRequested();
                    DataAdapter.FillSchema(DataSet, SchemaType.Source, "Report");
                    return DataSet.Tables["Report"];
                }
            }
            catch (PostgresException PostgresException)
            {
                throw new DatabaseException(PostgresException.Message, PostgresException.InnerException, PostgresException.Position);
            }
            catch (AggregateException AggregateException)
            {
                throw new DatabaseAggregateException(AggregateException.InnerExceptions);
            }
            catch (Exception Exception)
            {
                throw new DatabaseException(Exception.Message, Exception.InnerException);
            }
        }

        public async Task<List<ReportRow>> GetDataAsync(long company, string timeZone, string query, List<ReportColumns> columns, IEnumerable<Parameter> parameters, CancellationToken cancellationToken)
        {
            try
            {
                var Result = new List<ReportRow>();
                using (var Connection = await GetConnectionAsync(cancellationToken))
                using (var Command = new NpgsqlCommand(query, Connection))
                {
                    Command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.Name, p.Value)).ToArray());
                    cancellationToken.ThrowIfCancellationRequested();
                    using (var DataReader = await Command.ExecuteReaderAsync(cancellationToken))
                        if (DataReader.HasRows)
                        {
                            var numberTotals = new List<decimal?>();
                            var stringTotals = new List<string>();
                            var numberFinalTotals = new List<decimal?>();
                            var stringFinalTotals = new List<string>();
                            var distinctValues = new HashSet<string>();
                            int rowsCount = 0;
                            var ValuesRow = new List<object>();
                            //bool NeedToAddTotal = false;
                            while (DataReader.Read())
                            {
                                ++rowsCount;
                                var ReportRow = new ReportRow { Record = new List<string>() };
                                for (var Index = 0; Index < DataReader.FieldCount; ++Index)
                                {
                                    var column = columns[Index];
                                    var format = string.IsNullOrEmpty(column.Format) ? null : column.Format;
                                    object value;
                                    string formattedValue = null;
                                    decimal? numberValue = null;
                                    cancellationToken.ThrowIfCancellationRequested();
                                    if (!await DataReader.IsDBNullAsync(Index, cancellationToken))
                                    {
                                        var typeName = DataReader.GetDataTypeName(Index);
                                        switch (typeName)
                                        {
                                            case "timestamptz":
                                                var dateTimeOffsetValue = await DataReader.GetFieldValueAsync<DateTimeOffset>(Index, cancellationToken);
                                                value = dateTimeOffsetValue;
                                                formattedValue = dateTimeOffsetValue.Format(timeZone, format);
                                                break;
                                            case "int4":
                                            case "int8":
                                            case "decimal":
                                                numberValue = await DataReader.GetFieldValueAsync<decimal>(Index, cancellationToken);
                                                value = numberValue;
                                                formattedValue = format == null ? numberValue.ToString() : string.Format($"{{0:{format}}}", numberValue);
                                                break;
                                            case "bool":
                                                var booleanValue = await DataReader.GetFieldValueAsync<bool>(Index, cancellationToken);
                                                value = booleanValue;
                                                formattedValue = booleanValue ? "X" : string.Empty;
                                                break;
                                            default:
                                                value = formattedValue = await DataReader.GetFieldValueAsync<string>(Index, cancellationToken);
                                                break;
                                        }
                                    }

                                    /*
                                    switch (column.GroupAggregation)
                                    {
                                        case "sum":
                                        case "average":
                                            numberTotals[Index] = (numberTotals[Index] ?? 0) + (numberValue ?? 0);
                                            numberFinalTotals[Index] = (numberFinalTotals[Index] ?? 0) + (numberValue ?? 0);
                                            break;
                                        case "count":
                                            distinctValues.Add(formattedValue);
                                            break;
                                        case "min":
                                            if (numberValue.HasValue)
                                                numberTotals[Index] = numberTotals[Index] < numberValue ? numberTotals[Index] : numberValue;
                                            else
                                                stringTotals[Index] = string.Compare(stringTotals[Index] ?? "", formattedValue ?? "") < 0 ? stringTotals[Index] : formattedValue;
                                            break;
                                        case "max":
                                            if (numberValue.HasValue)
                                                numberTotals[Index] = numberTotals[Index] > numberValue ? numberTotals[Index] : numberValue;
                                            else
                                                stringTotals[Index] = string.Compare(stringTotals[Index] ?? "", formattedValue ?? "") > 0 ? stringTotals[Index] : formattedValue;
                                            break;
                                        case "first":
                                            if (numberValue.HasValue && !numberTotals[Index].HasValue)
                                                numberTotals[Index] = numberValue;
                                            else if (stringTotals[Index] == null)
                                                stringTotals[Index] = formattedValue;
                                            break;
                                        case "last":
                                            if (numberValue.HasValue)
                                                numberTotals[Index] = numberValue;
                                            else
                                                stringTotals[Index] = formattedValue;
                                            break;
                                    }

                                    */
                                    ReportRow.Record.Add(formattedValue ?? string.Empty);
                                }
                                Result.Add(ReportRow);
                            }
                        }
                }
                return Result;
            }
            catch (PostgresException PostgresException)
            {
                throw new DatabaseException(PostgresException.Message, PostgresException.InnerException, PostgresException.Position);
            }
            catch (AggregateException AggregateException)
            {
                throw new DatabaseAggregateException(AggregateException.InnerExceptions);
            }
            catch (Exception Exception)
            {
                throw new DatabaseException(Exception.Message, Exception.InnerException);
            }
        }
    }
}
