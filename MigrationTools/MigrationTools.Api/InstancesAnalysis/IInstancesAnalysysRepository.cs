using CheckListManager.Common.Data;
using CheckListManager.Instances.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CheckListManager.Instances.Data.InstancesAnalysis
{
    public interface IInstancesAnalysisRepository
    {
        Task ContinueAsync();
        Task RestartAsync();
        Task StopAsync();
        Task<InstancesAnalysisState> UpdateAsync();
    }
}