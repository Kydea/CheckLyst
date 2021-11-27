using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.AccessControl.Tests.Data
{
	public class UserRepositoryExtended : UserRepository, IUserRepositoryExtended
    {
        public UserRepositoryExtended(RelationalDatabaseEntities entities, ISettingService settingsService) : base(entities, settingsService)
		{
		}

		public Task CreateTestUser(Users user)
		{
			Entities.Users.Add(user);
			return Entities.SaveChangesAsync();
		}
	}
}
