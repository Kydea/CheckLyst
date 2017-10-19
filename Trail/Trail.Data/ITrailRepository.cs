using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.Trail.Data
{
	public interface ITrailRepository : IRepository
	{
		Task AddAsync(Records record);
	}
}
