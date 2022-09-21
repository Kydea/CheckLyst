using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal.InstancesAnalysis
{
    public interface IInstancesAnalysisService: IService
    {
        Task AnalyzeAsync(CancellationToken cancellationToken);
    }
}