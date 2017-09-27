using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.Audit.Data
{
	public class AuditRepository : Repository, IAuditRepository
	{
		public AuditRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public async Task AddAsync(Records record)
		{
			Entities.Records.Add(record);
			await SaveChangesAsync();
		}
	}
}
