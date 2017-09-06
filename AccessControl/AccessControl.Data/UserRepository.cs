using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public class UserRepository : Repository, IUserRepository
	{
		public UserRepository(RelationalDatabaseEntities entities) : base(entities)
		{			
		}

		public Users Get(string code)
		{
			return Entities.Users.FirstOrDefault(u => u.Code == code);
		}

		public async Task<Users> GetAsync(string code)
		{
			return await Entities.Users.FirstOrDefaultAsync(u => u.Code == code);
		}
	}
}