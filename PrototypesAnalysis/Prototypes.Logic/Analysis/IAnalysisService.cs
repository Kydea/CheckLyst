using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Prototypes.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.Analysis
{
    public interface IAnalysisService : IService
    {
        Task<AnalysisResponse> PrototypesAsync(DefaultIdentity identity, AnalysisRequest request, CancellationToken cancellationToken);
        Task<AnalysisResponse> InstancesAsync(DefaultIdentity identity, AnalysisRequest request, CancellationToken cancellationToken);
    }
}
