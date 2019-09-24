using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Api.Instances
{
    public interface IInstancesService : IService
    {
        Task InvalidateCacheAsync(InvalidateCacheRequest request);
    }
}