using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Data.Models;
using CheckListManager.Reports.Logic.Run;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Run
{
    public interface IRunService : IService
    {
        Task<PagingResponse<List<RunRecord>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<ListRequest> form);
        Task<ListResponse> BindAsync(DefaultIdentity identity, ListRequest data);
        Task DeleteAsync(DefaultIdentity identity, RunRequest request);
    }
}
