using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Tests.Data
{
	public class UserRepositoryExtended : UserRepository, IUserRepositoryExtended
	{
		public UserRepositoryExtended(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public async Task CreateTestUser(Users user)
		{
			Entities.Users.Add(user);
			await Entities.SaveChangesAsync();
		}
	}
}
