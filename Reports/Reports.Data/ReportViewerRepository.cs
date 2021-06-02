using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.Exceptions;
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

        public async Task<DataTable> GetDataTableAsync(long company, string command, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var Connection = await GetConnectionAsync(cancellationToken))
                using (var Command = new NpgsqlCommand(command, Connection))
                using (var DataAdapter = new NpgsqlDataAdapter(Command))
                using (var DataSet = new DataSet())
                {
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

        public async Task<List<List<string>>> GetDataAsync(long company, string timeZone, string query, IEnumerable<Parameter> parameters, CancellationToken cancellationToken)
        {
            try
            {
                var Result = new List<List<string>>();
                using (var Connection = await GetConnectionAsync(cancellationToken))
                using (var Command = new NpgsqlCommand(query, Connection))
                {
                    Command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.Name, p.Value)).ToArray());
                    cancellationToken.ThrowIfCancellationRequested();
                    using (var DataReader = await Command.ExecuteReaderAsync(cancellationToken))
                        if (DataReader.HasRows)
                            while (DataReader.Read())
                            {
                                var Record = new List<string>();
                                for(var Index = 0; Index < DataReader.FieldCount; ++Index)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    if (await DataReader.IsDBNullAsync(Index, cancellationToken))
                                        Record.Add(string.Empty);
                                    else
                                        switch (DataReader.GetDataTypeName(Index))
                                        {
                                            case "timestamptz":
                                                Record.Add((await DataReader.GetFieldValueAsync<DateTimeOffset>(Index, cancellationToken)).Format(timeZone));
                                                break;
                                            default:
                                                Record.Add(await DataReader.GetFieldValueAsync<string>(Index, cancellationToken));
                                                break;
                                        }
                                }
                                Result.Add(Record);
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
