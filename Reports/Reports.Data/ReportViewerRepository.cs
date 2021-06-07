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
                    if (parameters != null) Command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.Key, GetDefaultParameterValue(p.Value))).ToArray());
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

        private ReportRow AddTotalRow(int columnsCount, int rowsCount, List<ReportColumns> columns, List<string> columnTypes, List<decimal?> numberTotals, List<string> stringTotals, List<HashSet<string>> distinctValues)
        {
            for (var Index = 0; Index < columnsCount; ++Index)
            {
                var column = columns[Index];
                switch (column.GroupAggregation)
                {
                    case "average":
                        if (numberTotals[Index].HasValue)
                            numberTotals[Index] /= rowsCount;
                        break;
                    case "count":
                        numberTotals[Index] = distinctValues[Index].Count();
                        break;
                }
            }

            var ReportRow = new ReportRow { Total = true, Record = new List<string>() };
            for (var Index = 0; Index < columnsCount; ++Index)
            {
                ReportRow.Record.Add(stringTotals[Index] ?? numberTotals[Index]?.ToString() ?? string.Empty);
                numberTotals[Index] = null;
                stringTotals[Index] = null;
                distinctValues[Index] = null;
            }
            return ReportRow;
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
                            var firstRow = true;
                            int columnsCount = 0;
                            var columnTypes = new List<string>();
                            var numberRow = new List<decimal?>();
                            var stringRow = new List<string>();
                            var numberTotals = new List<decimal?>();
                            var stringTotals = new List<string>();
                            var numberFinalTotals = new List<decimal?>();
                            var stringFinalTotals = new List<string>();
                            var distinctValues = new List<HashSet<string>>();
                            var distinctFinalValues = new List<HashSet<string>>();
                            int rowsCount = 0;
                            bool anyGroupColumn = false;
                            bool needToAddTotal = false;
                            while (DataReader.Read())
                            {
                                ++rowsCount;
                                needToAddTotal = false;
                                var ReportRow = new ReportRow { Record = new List<string>() };
                                if (firstRow)
                                {
                                    columnsCount = Math.Min(DataReader.FieldCount, columns.Count);
                                    for (var Index = 0; Index < columnsCount; ++Index)
                                    {
                                        columnTypes.Add(DataReader.GetDataTypeName(Index));

                                        numberRow.Add(null);
                                        stringRow.Add(null);
                                        numberTotals.Add(null);
                                        stringTotals.Add(null);
                                        numberFinalTotals.Add(null);
                                        stringFinalTotals.Add(null);
                                        distinctValues.Add(null);
                                        distinctFinalValues.Add(null);
                                        stringRow.Add(null);
                                    }
                                    firstRow = false;
                                }

                                for (var Index = 0; Index < columnsCount; ++Index)
                                {
                                    var column = columns[Index];
                                    var format = string.IsNullOrEmpty(column.Format) ? null : column.Format;
                                    string formattedValue = null;
                                    cancellationToken.ThrowIfCancellationRequested();

                                    if (!await DataReader.IsDBNullAsync(Index, cancellationToken))
                                    {
                                        switch (columnTypes[Index])
                                        {
                                            case "timestamptz":
                                                var dateTimeOffsetValue = await DataReader.GetFieldValueAsync<DateTimeOffset>(Index, cancellationToken);
                                                formattedValue = dateTimeOffsetValue.Format(timeZone, format);
                                                break;
                                            case "int2":
                                            case "int4":
                                            case "int8":
                                            case "float8":
                                            case "decimal":
                                                var numberValue = await DataReader.GetFieldValueAsync<decimal>(Index, cancellationToken);
                                                numberRow[Index] = numberValue;
                                                formattedValue = format == null ? numberValue.ToString() : string.Format($"{{0:{format}}}", numberValue);
                                                break;
                                            case "bool":
                                                var booleanValue = await DataReader.GetFieldValueAsync<bool>(Index, cancellationToken);
                                                formattedValue = booleanValue.ToString();
                                                break;
                                            case "interval":
                                                var intervalValue = await DataReader.GetFieldValueAsync<TimeSpan>(Index, cancellationToken);
                                                formattedValue = intervalValue.ToString(@"dd\.hh\:mm\:ss");
                                                break;
                                            default:
                                                formattedValue = await DataReader.GetFieldValueAsync<string>(Index, cancellationToken);
                                                break;
                                        }
                                    }

                                    if (!needToAddTotal && column.GroupColumn && formattedValue != stringRow[Index])
                                    {
                                        anyGroupColumn = true;
                                        needToAddTotal = stringRow[Index] != null;
                                    }
                                    stringRow[Index] = formattedValue;

                                    ReportRow.Record.Add(formattedValue ?? string.Empty);
                                }

                                if (needToAddTotal)
                                {
                                    Result.Add(AddTotalRow(columnsCount, rowsCount - 1, columns, columnTypes, numberTotals, stringTotals, distinctValues));
                                    rowsCount = 1;
                                }

                                for (var Index = 0; Index < columnsCount; ++Index)
                                {
                                    var column = columns[Index];
                                    switch (column.GroupAggregation)
                                    {
                                        case "sum":
                                        case "average":
                                            numberTotals[Index] = (numberTotals[Index] ?? 0) + (numberRow[Index] ?? 0);
                                            numberFinalTotals[Index] = (numberFinalTotals[Index] ?? 0) + (numberRow[Index] ?? 0);
                                            break;
                                        case "count":
                                            if (distinctValues[Index] == null) distinctValues[Index] = new HashSet<string>();
                                            distinctValues[Index].Add(stringRow[Index]);
                                            if (distinctFinalValues[Index] == null) distinctFinalValues[Index] = new HashSet<string>();
                                            distinctFinalValues[Index].Add(stringRow[Index]);
                                            break;
                                        case "min":
                                            if (numberRow[Index].HasValue)
                                                numberTotals[Index] = numberTotals[Index] < numberRow[Index] ? numberTotals[Index] : numberRow[Index];
                                            else
                                                stringTotals[Index] = string.Compare(stringTotals[Index] ?? "", stringRow[Index] ?? "") < 0 ? stringTotals[Index] : stringRow[Index];
                                            break;
                                        case "max":
                                            if (numberRow[Index].HasValue)
                                                numberTotals[Index] = numberTotals[Index] > numberRow[Index] ? numberTotals[Index] : numberRow[Index];
                                            else
                                                stringTotals[Index] = string.Compare(stringTotals[Index] ?? "", stringRow[Index] ?? "") > 0 ? stringTotals[Index] : stringRow[Index];
                                            break;
                                        case "first":
                                            if (numberRow[Index].HasValue && !numberTotals[Index].HasValue)
                                                numberTotals[Index] = numberRow[Index];
                                            else if (stringTotals[Index] == null)
                                                stringTotals[Index] = stringRow[Index];
                                            break;
                                        case "last":
                                            if (numberRow[Index].HasValue)
                                                numberTotals[Index] = numberRow[Index];
                                            else
                                                stringTotals[Index] = stringRow[Index];
                                            break;
                                    }
                                }

                                Result.Add(ReportRow);
                            }

                            if (anyGroupColumn)
                            {
                                Result.Add(AddTotalRow(columnsCount, rowsCount, columns, columnTypes, numberTotals, stringTotals, distinctValues));
                                Result.Add(AddTotalRow(columnsCount, Result.Count, columns, columnTypes, numberFinalTotals, stringFinalTotals, distinctFinalValues));
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
