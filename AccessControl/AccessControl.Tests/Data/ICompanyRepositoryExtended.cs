using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Tests.Data
{
	public interface ICompanyRepositoryExtended : IRepository
	{
		Task<Companies> GetFirstCompanyAsync();
	}
}
