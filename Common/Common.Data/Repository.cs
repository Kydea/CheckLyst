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

		protected async Task<int> SaveChangesAsync()
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
