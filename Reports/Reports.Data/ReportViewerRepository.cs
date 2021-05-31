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

        private StringBuilder BuildQuery(long company, Common.Data.Reports report)
        {
            var StringBuilder = new StringBuilder();
            StringBuilder.AppendLine($"set role \"ReportUser{company}\";");
            StringBuilder.AppendLine($"with q as (");
            StringBuilder.AppendLine(report.Query);
            StringBuilder.AppendLine($")");
            StringBuilder.AppendLine($"select {string.Join(", ", report.ReportColumns.Select(c => $"{c.Column} \"{c.Label}\""))} from q");
            StringBuilder.AppendLine($"where");
            StringBuilder.AppendLine($"    1 = 1");
            StringBuilder.AppendLine($"    and q.\"CreatedOn\" >= @from");
            StringBuilder.AppendLine($"limit 500");
            return StringBuilder;
        }

        public async Task<List<List<string>>> GetDataAsync(long company, string timeZone, Common.Data.Reports report, IEnumerable<Parameter> parameters, CancellationToken cancellationToken)
        {
            try
            {
                var Result = new List<List<string>>();
                var Columns = report.ReportColumns.ToList();
                using (var Connection = await GetConnectionAsync(cancellationToken))
                using (var Command = new NpgsqlCommand(BuildQuery(company, report).ToString(), Connection))
                {
                    Command.Parameters.AddRange(parameters.Select(p => new NpgsqlParameter(p.Name, p.Value)).ToArray());
                    using (var DataReader = await Command.ExecuteReaderAsync(cancellationToken))
                        if (DataReader.HasRows)
                            while (DataReader.Read())
                            {
                                var Record = new List<string>();
                                var Index = 0;
                                foreach (var Column in Columns)
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
                                    ++Index;
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
