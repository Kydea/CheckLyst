using CheckListManager.Common.Data;
using CheckListManager.Common.Data.Exceptions;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Data
{
    public class ReportViewerRepository : Repository, IReportViewerRepository
    {
        public ReportViewerRepository(RelationalDatabaseEntities entities): base(entities)
        {
            var DatabaseConnection = entities.Database.Connection;

        }

        private async Task<NpgsqlConnection> GetConnection()
        {
            if (Entities.Database.Connection.State != ConnectionState.Open) await Entities.Database.Connection.OpenAsync();
            return (NpgsqlConnection)Entities.Database.Connection;
        }

        private object GetDefaultParameterValue(Parameter Parameter)
        {
            return string.Empty;
        }

        public async Task<DataTable> GetDataTable(long company, string command)
        {
            try
            {
                using (var Connection = await GetConnection())
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
    }
}
