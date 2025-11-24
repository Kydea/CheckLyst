using CheckListManager.Common.Core.Exceptions;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
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
        }

        private async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ReportDatabase"].ConnectionString;
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        private object GetDefaultParameterValue(string DataType) =>
            DataType == "Date" || DataType == "DateTime" ? new DateTime(2000, 1, 1) :
            DataType == "Number" ? 0 :
            DataType == "Boolean" ? false :
            (object)string.Empty;

        public async Task<DataTable> GetDataTableAsync(long company, string command, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
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
            catch (PostgresException postgresException)
            {
                throw new DatabaseException(postgresException.MessageText, postgresException) { Position = postgresException.Position };
            }
        }

        private ReportRow AddTotalRow(int columnsCount, int rowsCount, string decimalSeparator, List<ReportColumns> columns, List<string> columnTypes, List<bool> numericColumns, List<decimal?> numberTotals, List<string> stringTotals, List<HashSet<string>> distinctValues)
        {
            for (var Index = 0; Index < columnsCount; ++Index)
            {
                var column = columns[Index];
                switch (column.GroupAggregation)
                {
                    case "average":
                        if (numericColumns[Index])
                            numberTotals[Index] /= rowsCount;
                        break;
                    case "count":
                        numberTotals[Index] = distinctValues[Index]?.Count();
                        break;
                }
            }

            var ReportRow = new ReportRow { Total = true, Record = new List<string>() };
            for (var Index = 0; Index < columnsCount; ++Index)
            {
                var column = columns[Index];
                if (column.Visible)
                {
                    ReportRow.Record.Add(stringTotals[Index] ?? DecimalHelper.Format(numberTotals[Index], string.IsNullOrEmpty(column.Format) ? null : column.Format, decimalSeparator)?.ToString() ?? string.Empty);
                    numberTotals[Index] = null;
                    stringTotals[Index] = null;
                    distinctValues[Index] = null;
                }
            }
            return ReportRow;
        }

        public async Task<List<ReportRow>> GetDataAsync(long company, string timeZone, string decimalSeparator, string query, List<ReportColumns> columns, IEnumerable<Parameter> parameters, CancellationToken cancellationToken)
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
                        var numericColumns = new List<bool>();
                        var numberRow = new List<decimal?>();
                        var stringRow = new List<string>();
                        var numberTotals = new List<decimal?>();
                        var stringTotals = new List<string>();
                        var numberFinalTotals = new List<decimal?>();
                        var stringFinalTotals = new List<string>();
                        var distinctValues = new List<HashSet<string>>();
                        var distinctFinalValues = new List<HashSet<string>>();
                        int rowsCount = 0;
                        int totalRowsCount = 0;
                        bool anyGroupColumn = false;
                        bool needToAddTotal = false;
                        while (DataReader.Read())
                        {
                            ++rowsCount;
                            ++totalRowsCount;
                            needToAddTotal = false;
                            var ReportRow = new ReportRow { Record = new List<string>() };
                            if (firstRow)
                            {
                                columnsCount = Math.Min(DataReader.FieldCount, columns.Count);
                                for (var Index = 0; Index < columnsCount; ++Index)
                                {
                                    var columnType = DataReader.GetDataTypeName(Index);
                                    columnTypes.Add(columnType);
                                    numericColumns.Add(columnType == "int2" || columnType == "int4" || columnType == "int8" || columnType == "decimal" || columnType == "numeric" || columnType == "float8");
                                    if (columns[Index].GroupColumn) anyGroupColumn = true;

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
                            }

                            for (var Index = 0; Index < columnsCount; ++Index)
                            {
                                var column = columns[Index];
                                var format = string.IsNullOrEmpty(column.Format) ? null : column.Format;
                                string formattedValue = null;
                                numberRow[Index] = null;
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
                                        case "decimal":
                                        case "numeric":
                                            var numberValue = await DataReader.GetFieldValueAsync<decimal>(Index, cancellationToken);
                                            numberRow[Index] = numberValue;
                                            formattedValue = format == null ? numberValue.ToString() : DecimalHelper.Format(numberValue, format, decimalSeparator);
                                            break;
                                        case "float8":
                                            var floatValue = await DataReader.GetFieldValueAsync<double>(Index, cancellationToken);
                                            numberRow[Index] = (decimal)floatValue;
                                            formattedValue = format == null ? floatValue.ToString() : DecimalHelper.Format(floatValue, format, decimalSeparator);
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

                                if (column.GroupColumn && formattedValue != stringRow[Index] && !firstRow)
                                    needToAddTotal = true;

                                stringRow[Index] = formattedValue;

                                if (column.Visible)
                                    ReportRow.Record.Add(formattedValue ?? string.Empty);
                            }

                            if (needToAddTotal)
                            {
                                Result.Add(AddTotalRow(columnsCount, rowsCount - 1, decimalSeparator, columns, columnTypes, numericColumns, numberTotals, stringTotals, distinctValues));
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
                                        if (numericColumns[Index])
                                        {
                                            if (numberRow[Index].HasValue)
                                            {
                                                numberTotals[Index] = numberTotals[Index] < numberRow[Index] ? numberTotals[Index] : numberRow[Index];
                                                numberFinalTotals[Index] = numberFinalTotals[Index] < numberRow[Index] ? numberFinalTotals[Index] : numberRow[Index];
                                            }
                                        }
                                        else
                                        {
                                            stringTotals[Index] = stringTotals[Index] != null && string.Compare(stringTotals[Index] ?? "", stringRow[Index] ?? "") < 0 ? stringTotals[Index] : stringRow[Index];
                                            stringFinalTotals[Index] = stringFinalTotals[Index] != null && string.Compare(stringFinalTotals[Index] ?? "", stringRow[Index] ?? "") < 0 ? stringFinalTotals[Index] : stringRow[Index];
                                        }
                                        break;
                                    case "max":
                                        if (numericColumns[Index])
                                        {
                                            if (numberRow[Index].HasValue)
                                            {
                                                numberTotals[Index] = numberTotals[Index] > numberRow[Index] ? numberTotals[Index] : numberRow[Index];
                                                numberFinalTotals[Index] = numberFinalTotals[Index] > numberRow[Index] ? numberFinalTotals[Index] : numberRow[Index];
                                            }
                                        }
                                        else
                                        {
                                            stringTotals[Index] = string.Compare(stringTotals[Index] ?? "", stringRow[Index] ?? "") > 0 ? stringTotals[Index] : stringRow[Index];
                                            stringFinalTotals[Index] = string.Compare(stringFinalTotals[Index] ?? "", stringRow[Index] ?? "") > 0 ? stringFinalTotals[Index] : stringRow[Index];
                                        }
                                        break;
                                    case "first":
                                        if (numericColumns[Index])
                                        {
                                            if (!numberTotals[Index].HasValue)
                                                numberTotals[Index] = numberRow[Index];
                                            if (!numberFinalTotals[Index].HasValue)
                                                numberFinalTotals[Index] = numberRow[Index];
                                        }
                                        else
                                        {
                                            if (stringTotals[Index] == null)
                                                stringTotals[Index] = stringRow[Index];
                                            if (stringFinalTotals[Index] == null)
                                                stringFinalTotals[Index] = stringRow[Index];
                                        }
                                        break;
                                    case "last":
                                        if (numericColumns[Index])
                                        {
                                            numberTotals[Index] = numberRow[Index];
                                            numberFinalTotals[Index] = numberRow[Index];
                                        }
                                        else
                                        {
                                            stringTotals[Index] = stringRow[Index];
                                            stringFinalTotals[Index] = stringRow[Index];
                                        }
                                        break;
                                }
                            }

                            Result.Add(ReportRow);
                            firstRow = false;
                        }

                        if (anyGroupColumn)
                        {
                            Result.Add(AddTotalRow(columnsCount, rowsCount, decimalSeparator, columns, columnTypes, numericColumns, numberTotals, stringTotals, distinctValues));
                            Result.Add(AddTotalRow(columnsCount, totalRowsCount, decimalSeparator, columns, columnTypes, numericColumns, numberFinalTotals, stringFinalTotals, distinctFinalValues));
                        }
                    }
            }
            return Result;
        }
    }
}
