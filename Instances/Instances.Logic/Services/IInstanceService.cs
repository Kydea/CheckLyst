using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Instances.Logic.Messages;

namespace CheckListManager.Instances.Logic.Services
{
	public interface IInstanceService : IService
	{
		Task<CreateInstanceResponse> Create(int prototype, int version);
	}
}
