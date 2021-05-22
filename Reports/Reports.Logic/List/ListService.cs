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

        public async Task<List<ReportSource>> GetReportSourcesAsync() =>
            (await _reportsRepository.GetReportSourcesAsync()).Select(rs => new ReportSource
            {
                Code = rs.Code,
                Query = rs.Query
            }).ToList();

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> Form)
        {
            var Result = await _reportsRepository.GetReportsAsync(
                Company,
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
                    Code = r.Code,
                    Description = r.Description,
                    ReportSource = r.ReportSource,
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
