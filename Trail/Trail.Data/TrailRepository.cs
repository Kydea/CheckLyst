using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.Trail.Data
{
	public class TrailRepository : Repository, ITrailRepository
	{
		public TrailRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public async Task AddAsync(Records record)
		{
			Entities.Records.Add(record);
			await SaveChangesAsync();
		}
	}
}
