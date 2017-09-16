using System;

namespace CheckListManager.Common.Data
{
	public abstract class Repository : IRepository, IDisposable
	{
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
				Entities.Dispose();
			}
		}
	}
}
