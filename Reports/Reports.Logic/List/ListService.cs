using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.List
{
    public class ListService : IListService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IReportsRepository _reportsRepository;

        public ListService(ICommonRepository commonRepository, IReportsRepository reportsRepository)
        {
            _commonRepository = commonRepository;
            _reportsRepository = reportsRepository;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(IDefaultIdentity Identity, PagingRequest<ListRequest> Form)
        {
            var Result = await _reportsRepository.GetReportsAsync(
                Identity.CompanyId,
                Identity.HasPermission("Reports", "Edit") ? string.Empty : Identity.Code,
                Form.Data.ID,
                Form.Data.Code,
                Form.Data.Description,
                Form.CurrentPage,
                Form.ItemsPerPage
            );

            var Response = new PagingResponse<List<ListRow>>
            {
                Count = Result.Count,
                CurrentPage = Result.CurrentPage,
                ItemsPerPage = Result.ItemsPerPage,
                Data = Result.Data.Select(r => new ListRow
                {
                    ID = r.ID,
                    Code = r.Code,
                    Description = r.Description,
                    ReportSource = r.ReportSource,
                    Online = r.Online,
                    Restricted = r.Restricted,
                    CreatedBy = r.CreatedBy,
                    CreatedOn = r.CreatedOn,
                    ModifiedBy = r.ModifiedBy,
                    ModifiedOn = r.ModifiedOn
                }).ToList()
            };

            Response.Messages.AddRange(Response.Messages);

            return Response;
        }
    }
}
