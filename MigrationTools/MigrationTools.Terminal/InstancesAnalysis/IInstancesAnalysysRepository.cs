using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal.InstancesAnalysis
{
    public interface IInstancesAnalysisRepository
    {
        Task<IEnumerable<InstanceListItem>> GetInstancesAsync();
        Task<InstancesAnalysisState> GetStateAsync();
        Task StopAsync();
        Task OkAsync(long instance);
        Task ErrorAsync(string key, string error, long instance);
    }
}