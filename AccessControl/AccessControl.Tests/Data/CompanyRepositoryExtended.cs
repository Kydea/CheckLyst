using System.Data.Entity;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Tests.Data
{
	public class CompanyRepositoryExtended : Repository, ICompanyRepositoryExtended
	{
		public CompanyRepositoryExtended(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public async Task<Companies> GetFirstCompanyAsync()
			=> await Entities.Companies.FirstAsync();
	}
}
