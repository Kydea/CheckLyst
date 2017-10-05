using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.Audit.Data
{
	public interface IAuditRepository : IRepository
	{
		Task AddAsync(Records record);
	}
}
