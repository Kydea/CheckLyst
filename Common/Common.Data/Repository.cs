using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data.Exceptions;
using NLog;
using Npgsql;

namespace CheckListManager.Common.Data
{
	public abstract class Repository : IRepository, IDisposable
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		protected readonly RelationalDatabaseEntities Entities;

		protected Repository(RelationalDatabaseEntities entities)
		{
			Entities = entities;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// ReSharper disable once VirtualMemberNeverOverridden.Global
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Entities?.Dispose();
			}
		}

		public async Task<int> SaveChangesAsync()
		{
			try
			{
				return await Entities.SaveChangesAsync();
			}
			catch (DbUpdateException exception)
			{
				throw GetUpdateException(exception);
			}
			catch (DbEntityValidationException exception)
			{
				throw GetValidationErrors(exception);
			}
			catch (DBConcurrencyException)
			{
				throw;
			}
			catch (Exception e)
			{
				Logger.Error(e);
				throw;
			}
		}

		protected async Task<T> Call<T>(string name, params Parameter[] parameters)
		{
			if (Entities.Database.Connection.State != ConnectionState.Open)
			{
				await Entities.Database.Connection.OpenAsync();
			}

			using (var command = Entities.Database.Connection.CreateCommand())
			{
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = name;

				if (parameters != null)
				{
					foreach (var parameter in parameters)
					{
						var p = command.CreateParameter();
						p.ParameterName = parameter.Name;
						if (parameter.DbType.HasValue)
						{
							p.DbType = parameter.DbType.Value;
						}
						if (parameter.Value != null)
						{
							p.Value = parameter.Value;
						}
						command.Parameters.Add(p);
					}
				}

				return await command.ExecuteScalarAsync().ContinueWith(r => (T)r.Result);
			}
		}

		private static DatabaseAggregateException GetValidationErrors(DbEntityValidationException exception)
		{
			var exceptions =
				exception.EntityValidationErrors.SelectMany(
					v => v.ValidationErrors.Select(e => new DatabaseException(e.ErrorMessage)));
			
			return new DatabaseAggregateException(exceptions);
		}

		private static DatabaseException GetUpdateException(Exception updateException)
		{
			var exception = updateException;
			var type = typeof(PostgresException);
			while (exception.InnerException != null && exception.GetType() != type)
				exception = exception.InnerException;

			if (exception != null)
			{
				switch (((PostgresException)exception).SqlState)
				{
					case "23505":
						return new DatabaseException(DatabaseError.PrimaryKeyViolation, exception.Message, exception);
					default:
						return new DatabaseException(exception.Message, exception);
				}
			}

			return new DatabaseException(updateException.Message, updateException);
		}
	}
}
