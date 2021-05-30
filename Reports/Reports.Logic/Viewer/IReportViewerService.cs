using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Reports.Logic.Detail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Viewer
{
    public interface IViewerEditorService : IService
    {
        Task<DetailResponse> LoadParameters(IDefaultIdentity identity, string code);
        Task<ReportResponse> GetReportData(IDefaultIdentity identity, ReportRequest request);
    }
}
