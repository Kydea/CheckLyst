using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Prototypes.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.PrototypesAnalysis
{
    public interface IPrototypesAnalysisService : IService
    {
        Task StartAsync(DefaultIdentity identity, MigrationTools.Api.Monitor monitor, PrototypesAnalysisRequest request);
        InstanceCacheObject CreateInstanceAsync(PrototypeVersions prototypeVersion);
    }
}
