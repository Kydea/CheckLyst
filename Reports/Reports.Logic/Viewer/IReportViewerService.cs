using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Reports.Logic.Detail;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Viewer
{
    public interface IReportViewerService : IService
    {
        Task<DetailResponse> LoadParameters(IDefaultIdentity identity, long id);
        Task<ReportResponse> GetReportData(IDefaultIdentity identity, ReportRequest request, CancellationToken cancellationToken);
        Task<string> GetReportQuery(IDefaultIdentity identity, ReportRequest request, CancellationToken cancellationToken);
        Task EnqueueReportRun(DefaultIdentity identity, ReportRequest request);
        Task ToggleFavourite(IDefaultIdentity identity, ToggleFavouriteRequest request);
    }
}
