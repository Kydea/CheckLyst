using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceService : IService
	{
		Task<CreateInstanceResponse> Create(int prototype, int version);
		Task<SaveInstanceResponse> SaveAsync(long companyId, string userCode, string code, Instance instance);
	}
}
