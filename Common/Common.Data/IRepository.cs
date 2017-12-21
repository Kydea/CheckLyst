using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
	public interface IRepository
	{
        Task<int> SaveChangesAsync();
	}
}
