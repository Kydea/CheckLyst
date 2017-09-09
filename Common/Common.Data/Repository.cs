namespace CheckListManager.Common.Data
{
	public abstract class Repository : IRepository
	{
		protected readonly RelationalDatabaseEntities Entities;

		protected Repository(RelationalDatabaseEntities entities)
		{
			Entities = entities;
		}

		public void Dispose()
		{
			Entities.Dispose();
		}
	}
}
